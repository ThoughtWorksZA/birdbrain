using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using System.Web.Security;
using BirdBrain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdBrainTest
{
    [TestClass]
    public class ExtendedMembershipProviderTest
    {
        private EmbeddedBirdBrainExtendedMembershipProvider provider;

        [TestInitialize]
        public void Setup()
        {
            provider = new EmbeddedBirdBrainExtendedMembershipProvider();
            var section = (MembershipSection)ConfigurationManager.GetSection("system.web/membership");
            var config = section.Providers["BirdBrainMembership"].Parameters;
            provider.Initialize("MyApp", config);
        }

        [TestCleanup]
        public void Cleanup()
        {
            provider.Dispose();
        }

        [TestMethod]
        public void ShouldKnowOAuthIsNotSupported()
        {
            MembershipCreateStatus status;
            provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            Assert.AreEqual(0, provider.GetAccountsForUser("test").Count);
        }

        [TestMethod]
        public void ShouldKnowHowToCreateAnAccountWithConfirmationToken()
        {
            var confirmationToken = provider.CreateAccount("test", "password", true);
            var session = provider.DocumentStore.OpenSession();
            var usersQuery = from user in session.Query<User>()
                          where user.Username == "test"
                          select user;
            Assert.AreEqual(1, usersQuery.Count());
            Assert.AreEqual(confirmationToken, usersQuery.ToArray().First().ConfirmationToken);
        }

        [TestMethod]
        public void ShouldKnowToConfirmAccountWithValidConfirmationToken()
        {
            var confirmationToken = provider.CreateAccount("test", "password", true);
            Assert.IsTrue(provider.ConfirmAccount("test", confirmationToken));
        }

        [TestMethod]
        public void ShouldKnowToNotConfirmAccountWithInvalidConfirmationToken()
        {
            provider.CreateAccount("test", "password", true);
            Assert.IsFalse(provider.ConfirmAccount("test", Guid.NewGuid().ToString()));
        }

        [TestMethod]
        public void ShouldKnowToNotConfirmAccountWithNullUsername()
        {
            Assert.IsFalse(provider.ConfirmAccount(null, Guid.NewGuid().ToString()));
        }

        [TestMethod]
        public void ShouldKnowToNotConfirmAccountWithNullConfirmationToken()
        {
            provider.CreateAccount("test", "password", true);
            Assert.IsFalse(provider.ConfirmAccount("test", null));
        }

        [TestMethod]
        public void ShouldKnowToNotConfirmAccountWithInvalidUsername()
        {
            Assert.IsFalse(provider.ConfirmAccount("derp", null));
        }

        [TestMethod]
        public void ShouldKnowAccountIsConfirmed()
        {
            var confirmationToken = provider.CreateAccount("test", "password", true);
            Assert.IsTrue(provider.ConfirmAccount("test", confirmationToken));
            Assert.IsTrue(provider.IsConfirmed("test"));
        }

        [TestMethod]
        public void ShouldKnowAccountIsNotConfirmed()
        {
            provider.CreateAccount("test", "password", true);
            Assert.IsFalse(provider.IsConfirmed("test"));
        }

        [TestMethod]
        public void ShouldKnowANewAccountMayBeCreatedConfirmed()
        {
            provider.CreateAccount("test", "password", false);
            Assert.IsTrue(provider.IsConfirmed("test"));
        }

        [TestMethod]
        public void ShouldKnowAccountMayBeConfirmedUsingOnlyTheToken()
        {
            var confirmationToken = provider.CreateAccount("test", "password", true);
            Assert.IsTrue(provider.ConfirmAccount(confirmationToken));
            Assert.IsTrue(provider.IsConfirmed("test"));
        }

        [TestMethod]
        public void ShouldKnowHowToDeleteAccount()
        {
            provider.CreateAccount("test", "password", false);
            Assert.IsTrue(provider.DeleteAccount("test"));
            Thread.Sleep(500);
            var session = provider.DocumentStore.OpenSession();
            var usersQuery = from user in session.Query<User>()
                             where user.Username == "test"
                             select user;
            Assert.AreEqual(0, usersQuery.Count());
        }

        [TestMethod]
        public void ShouldKnowHowToGeneratePasswordResetToken()
        {
            provider.CreateAccount("test", "password", false);
            var beforeToken = DateTime.Now;
            var passwordResetToken = provider.GeneratePasswordResetToken("test", 60);
            Thread.Sleep(500);
            var session = provider.DocumentStore.OpenSession();
            var usersQuery = from _user in session.Query<User>()
                             where _user.Username == "test"
                             select _user;
            var user = usersQuery.ToArray().First();
            Assert.AreEqual(passwordResetToken, user.PasswordResetToken);
            Assert.IsTrue(beforeToken.Add(TimeSpan.FromMinutes(60)) < user.PasswordResetTokenExpiry);
        }

        [TestMethod]
        public void ShouldNotKnowHowToGeneratePasswordResetTokenForInvalidUser()
        {
            Assert.IsNull(provider.GeneratePasswordResetToken("test", 60));
            Assert.IsNull(provider.GeneratePasswordResetToken(null, 60));
        }

        [TestMethod]
        public void ShouldKnowHowToGetUserIdFromPasswordResetToken()
        {
            provider.CreateAccount("test", "password", false);
            var passwordResetToken = provider.GeneratePasswordResetToken("test", 60);
            Thread.Sleep(500);
            var userId = provider.GetUserIdFromPasswordResetToken(passwordResetToken);
            var session = provider.DocumentStore.OpenSession();
            var usersQuery = from _user in session.Query<User>()
                             where _user.Username == "test"
                             select _user;
            var user = usersQuery.ToArray().First();
            Assert.AreEqual(userId, user.GetIdAsInt());
        }

        [TestMethod]
        public void ShouldNotKnowHowToGetUserIdForInvalidPasswordResetToken()
        {
            Assert.AreEqual(-1, provider.GetUserIdFromPasswordResetToken("asdsadsadsadsasd"));
            Assert.AreEqual(-1, provider.GetUserIdFromPasswordResetToken(null));
        }

        [TestMethod]
        public void ShouldKnowHowToResetPasswordWithToken()
        {
            provider.CreateAccount("test", "password", false);
            var passwordResetToken = provider.GeneratePasswordResetToken("test", 60);
            Thread.Sleep(500);
            Assert.IsTrue(provider.ResetPasswordWithToken(passwordResetToken, "newpassword"));
            Thread.Sleep(500);
            Assert.IsTrue(provider.ValidateUser("test", "newpassword"));
        }

        [TestMethod]
        public void ShouldNotKnowHowToResetPasswordForInvalidToken()
        {
            const string passwordResetToken = "sometoken";
            Assert.IsFalse(provider.ResetPasswordWithToken(passwordResetToken, "newpassword"));
        }

        [TestMethod]
        public void ShouldNotKnowHowToResetPasswordWithTokenMoreThanOnce()
        {
            provider.CreateAccount("test", "password", false);
            var passwordResetToken = provider.GeneratePasswordResetToken("test", 60);
            Thread.Sleep(500);
            Assert.IsTrue(provider.ResetPasswordWithToken(passwordResetToken, "newpassword"));
            Thread.Sleep(500);
            Assert.IsFalse(provider.ResetPasswordWithToken(passwordResetToken, "newpassword"));
        }

        [TestMethod]
        public void ShouldKnowHowToReturnNumberOfPasswordFailures()
        {
            provider.CreateAccount("test", "password", false);
            provider.ValidateUser("test", "somepassword");
            Assert.AreEqual(1, provider.GetPasswordFailuresSinceLastSuccess("test"));
        }

        [TestMethod]
        public void ShouldKnowHowToGetCreateDate()
        {
            var beforeCreate = DateTime.Now;
            Thread.Sleep(100);
            provider.CreateAccount("test", "password", false);
            Assert.IsTrue(beforeCreate < provider.GetCreateDate("test"));
        }

        [TestMethod]
        public void ShouldNotKnowHowToGetCreateDateForInvalidUser()
        {
            Assert.AreEqual(DateTime.MinValue, provider.GetCreateDate("test"));
        }

        [TestMethod]
        public void ShouldKnowHowToGetLastPasswordChangedDate()
        {
            var beforeUpdate = DateTime.Now;
            provider.CreateAccount("test", "password", false);
            provider.ChangePassword("test", "password", "newpassword");
            Assert.IsTrue(beforeUpdate < provider.GetPasswordChangedDate("test"));
        }

        [TestMethod]
        public void ShouldKnowHowToGetLastPasswordChangedDateForTokenReset()
        {
            var beforeUpdate = DateTime.Now;
            provider.CreateAccount("test", "password", false);
            var passwordResetToken = provider.GeneratePasswordResetToken("test", 60);
            provider.ResetPasswordWithToken(passwordResetToken, "newpassword");
            Assert.IsTrue(beforeUpdate < provider.GetPasswordChangedDate("test"));
        }

        [TestMethod]
        public void ShouldKnowHowToGetLastPasswordChangedDateForRandomPasswordReset()
        {
            var beforeUpdate = DateTime.Now;
            provider.CreateAccount("test", "password", false);
            provider.ResetPassword("test", null);
            Assert.IsTrue(beforeUpdate < provider.GetPasswordChangedDate("test"));
        }

        [TestMethod]
        public void ShouldNotKnowHowToGetLastPasswordChangedDateForInvalidUser()
        {
            Assert.AreEqual(DateTime.MinValue, provider.GetPasswordChangedDate("test"));
        }

        [TestMethod]
        public void ShouldKnowHowToGetLastPasswordFailureDate()
        {
            var beforeUpdate = DateTime.Now;
            provider.CreateAccount("test", "password", false);
            provider.ValidateUser("test", "notpassword");
            Assert.IsTrue(beforeUpdate < provider.GetLastPasswordFailureDate("test"));
        }

        [TestMethod]
        public void ShouldNotKnowHowToGetLastPasswordFailureDateForInvalidUser()
        {
            Assert.AreEqual(DateTime.MinValue, provider.GetLastPasswordFailureDate("test"));
        }
    }
}
