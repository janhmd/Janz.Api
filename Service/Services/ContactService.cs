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
        private readonly IBaseRepository<ContactEntry> _entryRepo;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IValidator<CreateContactMessageDto> _createValidator;
        private readonly IValidator<ReplyContactMessageDto> _replyValidator;
        private readonly IValidator<FollowUpContactMessageDto> _followUpValidator;

        public ContactService(IBaseRepository<ContactMessage> messageRepo,
                              IBaseRepository<ContactEntry> entryRepo,
                              IEmailService emailService,
                              INotificationService notificationService,
                              IValidator<CreateContactMessageDto> createValidator,
                              IValidator<ReplyContactMessageDto> replyValidator,
                              IValidator<FollowUpContactMessageDto> followUpValidator)
        {
            _messageRepo = messageRepo;
            _entryRepo = entryRepo;
            _emailService = emailService;
            _notificationService = notificationService;
            _createValidator = createValidator;
            _replyValidator = replyValidator;
            _followUpValidator = followUpValidator;
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
            if (status == "new") messages = messages.Where(m => !m.IsAnswered && !m.IsClosed);
            else if (status == "answered") messages = messages.Where(m => m.IsAnswered);

            var entries = await LoadEntriesAsync();
            return messages.OrderByDescending(m => m.CreatedAt).Select(m => ToDto(m, entries)).ToList();
        }

        public async Task<ContactMessageDto> GetByIdAsync(int id)
        {
            var message = await _messageRepo.GetByIdAsync(id);
            if (message is null)
            {
                throw new NotFoundException();
            }

            return ToDto(message, await LoadEntriesAsync());
        }

        public async Task ReplyAsync(int id, ReplyContactMessageDto model)
        {
            await _replyValidator.EnsureValidAsync(model);

            var message = await _messageRepo.GetByIdAsync(id);
            if (message is null)
            {
                throw new NotFoundException();
            }

            if (message.IsClosed)
            {
                throw new BadRequestException("The customer ended this chat.");
            }

            if (message.IsAnswered)
            {
                throw new BadRequestException("This message has already been answered.");
            }

            var reply = model.Reply!.Trim();

            // Əvvəl məktub göndərilir: alınmasa mesaj "answered" olmur və yenidən cəhd etmək olar
            await _emailService.SendAsync(message.Email, $"Re: your message to Janz ({message.Topic})", BuildEmail(message, reply));

            // İlk cavab mesajın özündə, sonrakılar (müştəri yenidən yazandan sonra) yazışmanın davamında saxlanılır
            if (message.Reply is null)
            {
                message.Reply = reply;
                message.AnsweredAt = DateTime.UtcNow;
            }
            else
            {
                await _entryRepo.AddAsync(new ContactEntry { ContactMessageId = message.Id, IsStaff = true, Text = reply });
            }

            message.IsAnswered = true;
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
            var entries = await LoadEntriesAsync();
            return (await _messageRepo.GetAllAsync())
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => ToDto(m, entries))
                .ToList();
        }

        public async Task FollowUpAsync(string userId, int id, FollowUpContactMessageDto model)
        {
            await _followUpValidator.EnsureValidAsync(model);

            var message = await FindMineAsync(userId, id);
            if (message.IsClosed)
            {
                throw new BadRequestException("Support ended this chat. Please write a new message.");
            }

            if (!message.IsAnswered)
            {
                throw new BadRequestException("Support has not replied yet. You can write again after the reply.");
            }

            await _entryRepo.AddAsync(new ContactEntry { ContactMessageId = message.Id, IsStaff = false, Text = model.Text!.Trim() });

            // Yenidən "new" olur: support-un növbəsinə və admin bildirişlərinə düşür
            message.IsAnswered = false;
            await _messageRepo.UpdateAsync(message);
        }

        public async Task CloseAsync(int id)
        {
            var message = await _messageRepo.GetByIdAsync(id);
            if (message is null)
            {
                throw new NotFoundException();
            }

            if (message.IsClosed)
            {
                return;
            }

            // Cavabsız bağlananda da "new" siyahısından və admin bildirişlərindən çıxır
            message.IsClosed = true;
            message.ClosedAt = DateTime.UtcNow;
            message.IsAnswered = true;
            await _messageRepo.UpdateAsync(message);

            if (!string.IsNullOrEmpty(message.UserId))
            {
                await _notificationService.AddAsync(message.UserId, NotificationKind.Shop, "Support ended the chat.",
                    $"Your conversation about {message.Topic.ToLowerInvariant()} was closed. You can write a new message anytime.", "/Profile#messages");
            }
        }

        // Yalnız öz mesajı: başqasınınkı "tapılmadı" kimi görünür
        private async Task<ContactMessage> FindMineAsync(string userId, int id)
        {
            var message = await _messageRepo.GetByIdAsync(id);
            if (message is null || string.IsNullOrEmpty(message.UserId) || message.UserId != userId)
            {
                throw new NotFoundException();
            }

            return message;
        }

        private async Task<ILookup<int, ContactEntry>> LoadEntriesAsync()
        {
            return (await _entryRepo.GetAllAsync()).OrderBy(m => m.CreatedAt).ThenBy(m => m.Id).ToLookup(m => m.ContactMessageId);
        }

        private static string BuildEmail(ContactMessage message, string reply)
        {
            string Html(string text) => WebUtility.HtmlEncode(text).Replace("\n", "<br>");

            return $"<p>Hello {WebUtility.HtmlEncode(message.Name)},</p>"
                + $"<p>{Html(reply)}</p>"
                + $"<hr><p style=\"color:#777\">Your message:<br>{Html(message.Message)}</p>"
                + "<p>Janz</p>";
        }

        private static ContactMessageDto ToDto(ContactMessage message, ILookup<int, ContactEntry> entries) => new()
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
            CreatedAt = message.CreatedAt,
            Entries = entries[message.Id].Select(e => new ContactEntryDto { IsStaff = e.IsStaff, Text = e.Text, CreatedAt = e.CreatedAt }).ToList(),
            IsClosed = message.IsClosed,
            ClosedAt = message.ClosedAt,
            CanFollowUp = !string.IsNullOrEmpty(message.UserId) && message.IsAnswered && !message.IsClosed
        };
    }
}
