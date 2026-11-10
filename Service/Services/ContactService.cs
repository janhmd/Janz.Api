using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Contact;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;
using System.Net;

namespace Service.Services
{
    // Əlaqə formu: müştəri yazır, Admin və ya Support admin paneldən cavab verir, cavab müştərinin e-poçtuna gedir
    public class ContactService : IContactService
    {
        // Eyni e-poçtdan saatda bu qədər mesaj olarsa, yenisi qəbul olunmur (spam qoruması)
        private const int MaxPerHour = 3;

        private readonly IBaseRepository<ContactMessage> _messageRepo;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IValidator<CreateContactMessageDto> _createValidator;
        private readonly IValidator<ReplyContactMessageDto> _replyValidator;

        public ContactService(IBaseRepository<ContactMessage> messageRepo,
                              IEmailService emailService,
                              INotificationService notificationService,
                              IValidator<CreateContactMessageDto> createValidator,
                              IValidator<ReplyContactMessageDto> replyValidator)
        {
            _messageRepo = messageRepo;
            _emailService = emailService;
            _notificationService = notificationService;
            _createValidator = createValidator;
            _replyValidator = replyValidator;
        }

        public async Task CreateAsync(CreateContactMessageDto model, string? userId)
        {
            await _createValidator.EnsureValidAsync(model);

            var email = model.Email!.Trim();
            var since = DateTime.UtcNow.AddHours(-1);
            var recent = (await _messageRepo.GetAllAsync())
                .Count(m => m.CreatedAt >= since && string.Equals(m.Email, email, StringComparison.OrdinalIgnoreCase));
            if (recent >= MaxPerHour)
            {
                throw new BadRequestException("You have already sent several messages. Please wait a little before writing again.");
            }

            await _messageRepo.AddAsync(new ContactMessage
            {
                UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
                Name = model.Name!.Trim(),
                Email = email,
                Topic = model.Topic!,
                Message = model.Message!.Trim()
            });
        }

        public async Task<IEnumerable<ContactMessageDto>> GetAllAsync(string? status)
        {
            var messages = (await _messageRepo.GetAllAsync()).AsEnumerable();
            if (status == "new") messages = messages.Where(m => !m.IsAnswered);
            else if (status == "answered") messages = messages.Where(m => m.IsAnswered);

            return messages.OrderByDescending(m => m.CreatedAt).Select(ToDto).ToList();
        }

        public async Task<ContactMessageDto> GetByIdAsync(int id)
        {
            var message = await _messageRepo.GetByIdAsync(id);
            if (message is null)
            {
                throw new NotFoundException();
            }

            return ToDto(message);
        }

        public async Task ReplyAsync(int id, ReplyContactMessageDto model)
        {
            await _replyValidator.EnsureValidAsync(model);

            var message = await _messageRepo.GetByIdAsync(id);
            if (message is null)
            {
                throw new NotFoundException();
            }

            if (message.IsAnswered)
            {
                throw new BadRequestException("This message has already been answered.");
            }

            var reply = model.Reply!.Trim();

            // Əvvəl məktub göndərilir: alınmasa mesaj "answered" olmur və yenidən cəhd etmək olar
            await _emailService.SendAsync(message.Email, $"Re: your message to Janz ({message.Topic})", BuildEmail(message, reply));

            message.Reply = reply;
            message.IsAnswered = true;
            message.AnsweredAt = DateTime.UtcNow;
            await _messageRepo.UpdateAsync(message);

            // Giriş etmiş müştəriyə saytda da bildiriş gedir: e-poçt gəlməsə belə cavabı hesabında görür
            if (!string.IsNullOrEmpty(message.UserId))
            {
                await _notificationService.AddAsync(message.UserId, NotificationKind.Shop, "Support replied.",
                    $"We answered your message about {message.Topic.ToLowerInvariant()}.", "/Profile#messages");
            }
        }

        public async Task<IEnumerable<ContactMessageDto>> GetMineAsync(string userId)
        {
            return (await _messageRepo.GetAllAsync())
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(ToDto)
                .ToList();
        }

        private static string BuildEmail(ContactMessage message, string reply)
        {
            string Html(string text) => WebUtility.HtmlEncode(text).Replace("\n", "<br>");

            return $"<p>Hello {WebUtility.HtmlEncode(message.Name)},</p>"
                + $"<p>{Html(reply)}</p>"
                + $"<hr><p style=\"color:#777\">Your message:<br>{Html(message.Message)}</p>"
                + "<p>Janz</p>";
        }

        private static ContactMessageDto ToDto(ContactMessage message) => new()
        {
            Id = message.Id,
            Name = message.Name,
            Email = message.Email,
            Topic = message.Topic,
            Message = message.Message,
            IsMember = !string.IsNullOrEmpty(message.UserId),
            IsAnswered = message.IsAnswered,
            Reply = message.Reply,
            AnsweredAt = message.AnsweredAt,
            CreatedAt = message.CreatedAt
        };
    }
}
