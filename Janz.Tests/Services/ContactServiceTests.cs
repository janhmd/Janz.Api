using Domain.Entities;
using Janz.Tests.Fakes;
using Service.Helpers.DTOs.Contact;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators.Contact;
using Service.Services;
using Xunit;

namespace Janz.Tests.Services
{
    public class ContactServiceTests
    {
        private readonly FakeRepository<ContactMessage> _repo = new();
        private readonly FakeEmailService _email = new();
        private readonly FakeNotificationService _notifications = new();

        private ContactService Create() => new(_repo, _email, _notifications, new CreateContactMessageDtoValidator(), new ReplyContactMessageDtoValidator());

        private static CreateContactMessageDto Valid() => new()
        {
            Name = "  Aysel  ",
            Email = " aysel@example.com ",
            Topic = "Order",
            Message = "  My order has not arrived yet.  "
        };

        [Fact]
        public async Task Create_SavesTrimmedMessage_WithUserId()
        {
            await Create().CreateAsync(Valid(), "user-1");

            var saved = Assert.Single(_repo.Rows);
            Assert.Equal("Aysel", saved.Name);
            Assert.Equal("aysel@example.com", saved.Email);
            Assert.Equal("My order has not arrived yet.", saved.Message);
            Assert.Equal("user-1", saved.UserId);
            Assert.False(saved.IsAnswered);
        }

        [Fact]
        public async Task Create_WorksForGuests_WithoutUserId()
        {
            await Create().CreateAsync(Valid(), null);

            Assert.Null(Assert.Single(_repo.Rows).UserId);
        }

        [Theory]
        [InlineData("", "a@b.com", "Order", "A long enough message")]
        [InlineData("Aysel", "not-an-email", "Order", "A long enough message")]
        [InlineData("Aysel", "a@b.com", "Weather", "A long enough message")]
        [InlineData("Aysel", "a@b.com", "Order", "short")]
        [InlineData("Aysel", "a@b.com", "Order", "   ")]
        public async Task Create_RejectsInvalidInput(string name, string email, string topic, string message)
        {
            var model = new CreateContactMessageDto { Name = name, Email = email, Topic = topic, Message = message };

            await Assert.ThrowsAsync<BadRequestException>(() => Create().CreateAsync(model, null));
            Assert.Empty(_repo.Rows);
        }

        [Fact]
        public async Task Create_RejectsTooLongMessage()
        {
            var model = Valid();
            model.Message = new string('a', 2001);

            await Assert.ThrowsAsync<BadRequestException>(() => Create().CreateAsync(model, null));
        }

        [Fact]
        public async Task Create_BlocksSpam_FromTheSameEmail()
        {
            var service = Create();
            for (var i = 0; i < 3; i++)
            {
                await service.CreateAsync(Valid(), null);
            }

            await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAsync(Valid(), null));
            Assert.Equal(3, _repo.Rows.Count);
        }

        [Fact]
        public async Task GetAll_FiltersByStatus_NewestFirst()
        {
            _repo.Rows.Add(new ContactMessage { Id = 1, Name = "A", CreatedAt = DateTime.UtcNow.AddDays(-2) });
            _repo.Rows.Add(new ContactMessage { Id = 2, Name = "B", CreatedAt = DateTime.UtcNow.AddDays(-1), IsAnswered = true });
            _repo.Rows.Add(new ContactMessage { Id = 3, Name = "C", CreatedAt = DateTime.UtcNow });

            var service = Create();

            Assert.Equal(new[] { 3, 2, 1 }, (await service.GetAllAsync(null)).Select(m => m.Id));
            Assert.Equal(new[] { 3, 1 }, (await service.GetAllAsync("new")).Select(m => m.Id));
            Assert.Equal(new[] { 2 }, (await service.GetAllAsync("answered")).Select(m => m.Id));
        }

        [Fact]
        public async Task GetById_ThrowsNotFound_ForUnknownId()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => Create().GetByIdAsync(99));
        }

        [Fact]
        public async Task Reply_NotifiesTheMember_ButNotAGuest()
        {
            _repo.Rows.Add(new ContactMessage { Id = 1, UserId = "user-1", Name = "Aysel", Email = "aysel@example.com", Topic = "Order", Message = "Where is my order?" });
            _repo.Rows.Add(new ContactMessage { Id = 2, Name = "Guest", Email = "guest@example.com", Topic = "Other", Message = "A question for you" });

            await Create().ReplyAsync(1, new ReplyContactMessageDto { Reply = "It ships tomorrow." });
            await Create().ReplyAsync(2, new ReplyContactMessageDto { Reply = "Of course." });

            var added = Assert.Single(_notifications.Added);
            Assert.Equal("user-1", added.UserId);
            Assert.Equal("/Profile#messages", added.Link);
        }

        [Fact]
        public async Task GetMine_ReturnsOnlyOwnMessages_NewestFirst_WithReplies()
        {
            _repo.Rows.Add(new ContactMessage { Id = 1, UserId = "user-1", Name = "A", Email = "a@b.com", Topic = "Order", Message = "Older message here", CreatedAt = DateTime.UtcNow.AddDays(-2), IsAnswered = true, Reply = "Done." });
            _repo.Rows.Add(new ContactMessage { Id = 2, UserId = "user-1", Name = "A", Email = "a@b.com", Topic = "Other", Message = "Newer message here", CreatedAt = DateTime.UtcNow.AddDays(-1) });
            _repo.Rows.Add(new ContactMessage { Id = 3, UserId = "user-2", Name = "B", Email = "b@b.com", Topic = "Other", Message = "Someone else's one" });

            var mine = (await Create().GetMineAsync("user-1")).ToList();

            Assert.Equal(new[] { 2, 1 }, mine.Select(m => m.Id).ToArray());
            Assert.Equal("Done.", mine[1].Reply);
        }

        [Fact]
        public async Task Reply_SendsEmail_AndMarksAnswered()
        {
            _repo.Rows.Add(new ContactMessage { Id = 1, Name = "Aysel", Email = "aysel@example.com", Topic = "Order", Message = "Where is my order?" });

            await Create().ReplyAsync(1, new ReplyContactMessageDto { Reply = "  It ships tomorrow.  " });

            var mail = Assert.Single(_email.Sent);
            Assert.Equal("aysel@example.com", mail.To);
            Assert.Contains("It ships tomorrow.", mail.Html);
            Assert.Contains("Where is my order?", mail.Html);
            var message = _repo.Rows.Single();
            Assert.True(message.IsAnswered);
            Assert.Equal("It ships tomorrow.", message.Reply);
            Assert.NotNull(message.AnsweredAt);
        }

        [Fact]
        public async Task Reply_EncodesHtml_InTheEmail()
        {
            _repo.Rows.Add(new ContactMessage { Id = 1, Name = "<b>Aysel</b>", Email = "a@b.com", Topic = "Other", Message = "<script>alert(1)</script>" });

            await Create().ReplyAsync(1, new ReplyContactMessageDto { Reply = "<i>Hello</i>" });

            var html = Assert.Single(_email.Sent).Html;
            Assert.DoesNotContain("<script>", html);
            Assert.DoesNotContain("<i>Hello</i>", html);
        }

        [Fact]
        public async Task Reply_DoesNotMarkAnswered_WhenEmailFails()
        {
            _repo.Rows.Add(new ContactMessage { Id = 1, Name = "Aysel", Email = "a@b.com", Topic = "Order", Message = "Hello there friend" });
            _email.Fail = true;

            await Assert.ThrowsAsync<BadRequestException>(() => Create().ReplyAsync(1, new ReplyContactMessageDto { Reply = "Hi" }));

            Assert.False(_repo.Rows.Single().IsAnswered);
        }

        [Fact]
        public async Task Reply_Throws_WhenAlreadyAnswered()
        {
            _repo.Rows.Add(new ContactMessage { Id = 1, Name = "Aysel", Email = "a@b.com", Topic = "Order", Message = "Hello there friend", IsAnswered = true });

            await Assert.ThrowsAsync<BadRequestException>(() => Create().ReplyAsync(1, new ReplyContactMessageDto { Reply = "Hi again" }));
            Assert.Empty(_email.Sent);
        }

        [Fact]
        public async Task Reply_Throws_ForEmptyReplyOrUnknownMessage()
        {
            _repo.Rows.Add(new ContactMessage { Id = 1, Name = "Aysel", Email = "a@b.com", Topic = "Order", Message = "Hello there friend" });
            var service = Create();

            await Assert.ThrowsAsync<BadRequestException>(() => service.ReplyAsync(1, new ReplyContactMessageDto { Reply = "  " }));
            await Assert.ThrowsAsync<NotFoundException>(() => service.ReplyAsync(7, new ReplyContactMessageDto { Reply = "Hi" }));
        }
    }
}
