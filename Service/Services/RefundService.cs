using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Refunds;
using Service.Helpers.Exceptions;
using Service.Helpers.Responses;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    // Pulun geri qaytarılması: müştəri çatdırılmış sifariş üçün sorğu yazır, Admin və ya Support qəbul edəndə
    // məbləğ müştərinin daxili hesabına (balansa) qayıdır. Rədd edəndə səbəb müştəriyə bildirişlə gedir
    public class RefundService : IRefundService
    {
        private readonly IRefundRepository _refundRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly INotificationService _notificationService;
        private readonly IValidator<CreateRefundDto> _createValidator;
        private readonly IValidator<ResolveRefundDto> _resolveValidator;
        private readonly ILogger<RefundService> _logger;

        public RefundService(IRefundRepository refundRepo,
                             IOrderRepository orderRepo,
                             INotificationService notificationService,
                             IValidator<CreateRefundDto> createValidator,
                             IValidator<ResolveRefundDto> resolveValidator,
                             ILogger<RefundService> logger)
        {
            _refundRepo = refundRepo;
            _orderRepo = orderRepo;
            _notificationService = notificationService;
            _createValidator = createValidator;
            _resolveValidator = resolveValidator;
            _logger = logger;
        }

        public async Task<ServiceResult<RefundDto>> CreateAsync(string userId, int orderId, CreateRefundDto model)
        {
            var validation = await _createValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return ServiceResult<RefundDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).Distinct().ToArray());
            }

            var order = await _orderRepo.GetByIdForUserAsync(orderId, userId);
            if (order is null)
            {
                return ServiceResult<RefundDto>.NotFound();
            }

            var latest = await _refundRepo.GetLatestByOrderAsync(orderId);
            var error = CannotRequestReason(order, latest);
            if (error is not null)
            {
                return ServiceResult<RefundDto>.Fail(error);
            }

            var request = new RefundRequest
            {
                OrderId = orderId,
                UserId = userId,
                Reason = model.Reason!.Trim(),
                Amount = order.Total,
                Status = RefundStatus.Pending
            };
            await _refundRepo.AddAsync(request);
            _logger.LogInformation("Refund requested for order #{OrderId} by user {UserId}, amount {Amount}", orderId, userId, request.Amount);

            return ServiceResult<RefundDto>.Ok(ToDto(request));
        }

        public async Task<IEnumerable<AdminRefundDto>> GetAllForAdminAsync(string? status)
        {
            RefundStatus? filter = null;
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<RefundStatus>(status, true, out var parsed) || int.TryParse(status, out _))
                {
                    throw new BadRequestException("This status is not known.");
                }
                filter = parsed;
            }

            var requests = await _refundRepo.GetAllForAdminAsync(filter);
            return requests.Select(ToAdminDto);
        }

        public async Task<AdminRefundDto> GetByIdForAdminAsync(int id)
        {
            var request = await _refundRepo.GetByIdForAdminAsync(id);
            if (request is null)
            {
                throw new NotFoundException();
            }

            return ToAdminDto(request);
        }

        public async Task ApproveAsync(int id, ResolveRefundDto model)
        {
            await _resolveValidator.EnsureValidAsync(model);

            var request = await _refundRepo.GetByIdForAdminAsync(id);
            if (request is null)
            {
                throw new NotFoundException();
            }

            // Məbləğ boşdursa sifarişin cəmi qayıdır, sifarişin cəmindən çox ola bilməz
            var amount = model.Amount ?? request.Order.Total;
            if (amount > request.Order.Total)
            {
                throw new BadRequestException("The refund can't be more than the order total.");
            }

            var note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim();
            if (!await _refundRepo.ApproveAsync(id, amount, note))
            {
                throw new BadRequestException("This request was already handled by someone else. Refresh the page.");
            }

            _logger.LogInformation("Refund #{RefundId} for order #{OrderId} approved: {Amount} returned to the balance of user {UserId}", id, request.OrderId, amount, request.UserId);
            await _notificationService.AddAsync(request.UserId, NotificationKind.Shop,
                $"Refund for order #{request.OrderId} approved",
                $"{amount:0.00} was returned to your balance.", $"/Orders/Detail/{request.OrderId}");
        }

        public async Task RejectAsync(int id, ResolveRefundDto model)
        {
            await _resolveValidator.EnsureValidAsync(model);
            if (string.IsNullOrWhiteSpace(model.Note))
            {
                throw new BadRequestException("Write why the refund is rejected.");
            }

            var request = await _refundRepo.GetByIdForAdminAsync(id);
            if (request is null)
            {
                throw new NotFoundException();
            }

            if (!await _refundRepo.RejectAsync(id, model.Note.Trim()))
            {
                throw new BadRequestException("This request was already handled by someone else. Refresh the page.");
            }

            _logger.LogInformation("Refund #{RefundId} for order #{OrderId} rejected", id, request.OrderId);
            await _notificationService.AddAsync(request.UserId, NotificationKind.Shop,
                $"Refund for order #{request.OrderId} rejected",
                model.Note.Trim(), $"/Orders/Detail/{request.OrderId}");
        }

        // Sifariş səhifəsi üçün: sorğu göndərmək olarmı (bütün qaydalar bir yerdə)
        public static string? CannotRequestReason(Order order, RefundRequest? latest)
        {
            if (order.Status != OrderStatus.Delivered)
            {
                return "A refund can be requested only for a delivered order.";
            }

            if (order.Total <= 0)
            {
                return "There is nothing to refund for this order.";
            }

            if (latest is not null && latest.Status == RefundStatus.Pending)
            {
                return "A refund request for this order is already waiting for an answer.";
            }

            if (latest is not null && latest.Status == RefundStatus.Approved)
            {
                return "The money for this order was already returned to your balance.";
            }

            return null;
        }

        public static RefundDto ToDto(RefundRequest request)
        {
            var dto = new RefundDto();
            Fill(dto, request);
            return dto;
        }

        private static AdminRefundDto ToAdminDto(RefundRequest request)
        {
            var dto = new AdminRefundDto();
            Fill(dto, request);
            dto.UserId = request.UserId;
            dto.CustomerName = (request.Order.User.Name + " " + request.Order.User.Surname).Trim();
            dto.CustomerEmail = request.Order.User.Email ?? string.Empty;
            dto.OrderTotal = request.Order.Total;
            return dto;
        }

        private static void Fill(RefundDto dto, RefundRequest request)
        {
            dto.Id = request.Id;
            dto.OrderId = request.OrderId;
            dto.Reason = request.Reason;
            dto.Amount = request.Amount;
            dto.Status = request.Status.ToString();
            dto.StaffNote = request.StaffNote;
            dto.ResolvedAt = request.ResolvedAt;
            dto.CreatedAt = request.CreatedAt;
        }
    }
}
