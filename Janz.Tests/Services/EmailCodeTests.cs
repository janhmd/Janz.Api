using Service.Helpers;
using Xunit;

namespace Janz.Tests.Services
{
    public class EmailCodeTests
    {
        private static readonly DateTime Now = new(2026, 11, 6, 10, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void NewCode_HasFourDigits()
        {
            for (var i = 0; i < 200; i++)
            {
                var code = EmailCode.NewCode();

                Assert.Equal(4, code.Length);
                Assert.True(code.All(char.IsDigit));
            }
        }

        [Fact]
        public void Check_AcceptsTheRightCode()
        {
            var stored = EmailCode.Pack("u1", "0427", Now);

            var (result, _) = EmailCode.Check(stored, "u1", "0427", Now.AddMinutes(1));

            Assert.Equal(EmailCodeResult.Valid, result);
        }

        [Fact]
        public void Check_AcceptsTheCodeWithSpaces()
        {
            var stored = EmailCode.Pack("u1", "0427", Now);

            var (result, _) = EmailCode.Check(stored, "u1", " 0427 ", Now);

            Assert.Equal(EmailCodeResult.Valid, result);
        }

        [Fact]
        public void Check_RejectsTheWrongCode_AndCountsTheAttempt()
        {
            var stored = EmailCode.Pack("u1", "0427", Now);

            var (result, updated) = EmailCode.Check(stored, "u1", "1111", Now);

            Assert.Equal(EmailCodeResult.Wrong, result);
            Assert.EndsWith("|1", updated);
        }

        [Fact]
        public void Check_RejectsTheCodeOfAnotherUser()
        {
            var stored = EmailCode.Pack("u1", "0427", Now);

            var (result, _) = EmailCode.Check(stored, "u2", "0427", Now);

            Assert.Equal(EmailCodeResult.Wrong, result);
        }

        [Fact]
        public void Check_LocksAfterFiveWrongCodes_EvenForTheRightOne()
        {
            var stored = EmailCode.Pack("u1", "0427", Now);
            var result = EmailCodeResult.Wrong;

            for (var i = 0; i < EmailCode.MaxAttempts; i++)
            {
                (result, stored) = EmailCode.Check(stored, "u1", "1111", Now);
            }

            Assert.Equal(EmailCodeResult.Locked, result);
            Assert.Equal(EmailCodeResult.Locked, EmailCode.Check(stored, "u1", "0427", Now).Result);
        }

        [Fact]
        public void Check_RejectsAnExpiredCode()
        {
            var stored = EmailCode.Pack("u1", "0427", Now);

            var (result, _) = EmailCode.Check(stored, "u1", "0427", Now.AddMinutes(11));

            Assert.Equal(EmailCodeResult.Expired, result);
        }

        [Fact]
        public void Check_TreatsMissingCodeAsExpired()
        {
            Assert.Equal(EmailCodeResult.Expired, EmailCode.Check(null, "u1", "0427", Now).Result);
            Assert.Equal(EmailCodeResult.Expired, EmailCode.Check("garbage", "u1", "0427", Now).Result);
        }

        [Fact]
        public void Pack_DoesNotContainTheCode()
        {
            var stored = EmailCode.Pack("u1", "0427", Now);

            Assert.DoesNotContain("0427", stored);
        }
    }
}
