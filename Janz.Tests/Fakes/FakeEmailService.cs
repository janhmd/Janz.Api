using Service.Services.Interfaces;

namespace Janz.Tests.Fakes
{
    // Məktub göndərmir, yalnız yadda saxlayır: testdə nəyin göndərildiyini yoxlamaq üçün
    public class FakeEmailService : IEmailService
    {
        public List<(string To, string Subject, string Html)> Sent { get; } = new();

        public bool Fail { get; set; }

        public Task SendAsync(string to, string subject, string html)
        {
            if (Fail)
            {
                throw new Service.Helpers.Exceptions.BadRequestException("We could not send the email right now. Please try again later.");
            }

            Sent.Add((to, subject, html));
            return Task.CompletedTask;
        }
    }
}
