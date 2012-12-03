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
    public class MembershipProviderTest
    {
        private EmbeddedBirdBrainMembershipProvider provider;

        [TestInitialize]
        public void Setup()
        {
            provider = new EmbeddedBirdBrainMembershipProvider();
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
        public void ShouldKnowHowToCreateUser()
        {
            MembershipCreateStatus status;
            var membershipUser = provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            Assert.AreEqual(MembershipCreateStatus.Success, status);
            Assert.AreEqual("test", membershipUser.UserName);
            var session = provider.DocumentStore.OpenSession();
            var results = from user in session.Query<User>()
                          where user.Username == "test"
                          select user;
            Assert.AreEqual(1, results.Count());
        }

        [TestMethod]
        public void ShouldKnowHowToValidateUser()
        {
            MembershipCreateStatus status;
            provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            Assert.IsTrue(provider.ValidateUser("test", "password"));
        }

        [TestMethod]
        public void ShouldKnowHowToInvalidateUser()
        {
            MembershipCreateStatus status;
            provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            Assert.IsFalse(provider.ValidateUser("test", "notpassword"));
        }

        [TestMethod]
        public void ShouldKnowHowToChangePassword()
        {
            MembershipCreateStatus status;
            provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            Assert.IsTrue(provider.ChangePassword("test", "password", "newpassword"));
            Thread.Sleep(100);
            Assert.IsTrue(provider.ValidateUser("test", "newpassword"));
        }

        [TestMethod]
        public void ShouldKnowOldPasswordMustBeCorrectWhenChangingPassword()
        {
            MembershipCreateStatus status;
            provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            Assert.IsFalse(provider.ChangePassword("test", "notpassword", "newpassword"));
        }

        [TestMethod]
        public void ShouldKnowHowToGetAUserByUsername()
        {
            MembershipCreateStatus status;
            var createdUser = provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            var retrievedUser = provider.GetUser("test", true);
            Assert.AreEqual(createdUser.ProviderUserKey, retrievedUser.ProviderUserKey);
            Assert.AreEqual(createdUser.UserName, retrievedUser.UserName);
        }

        [TestMethod]
        public void ShouldKnowHowToDeleteUserExcludingRelatedData()
        {
            MembershipCreateStatus status;
            provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            Assert.IsTrue(provider.DeleteUser("test", false));
            var session = provider.DocumentStore.OpenSession();
            var results = from user in session.Query<User>()
                          where user.Username == "test"
                          select user;
            Assert.AreEqual(0, results.ToArray().Count());
        }

        [TestMethod]
        public void ShouldKnowHowToGetAUserByProviderUserKey()
        {
            MembershipCreateStatus status;
            var createdUser = provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            var retrievedUser = provider.GetUser(createdUser.ProviderUserKey, true);
            Assert.AreEqual(createdUser.ProviderUserKey, retrievedUser.ProviderUserKey);
            var result = createdUser.Equals(retrievedUser);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldKnowHowToGetUserByEmail()
        {
            MembershipCreateStatus status;
            var createdUser = provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            var retrievedUserName = provider.GetUserNameByEmail(createdUser.Email); 

            Assert.AreEqual(createdUser.UserName, retrievedUserName);
        }

        [TestMethod]
        public void ShouldKnowHowToChangePasswordQuestionAndAnswer()
        {
            MembershipCreateStatus status;
            provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            Assert.IsTrue(provider.ChangePasswordQuestionAndAnswer("test", "password", "Is this for real?", "no"));
            var session = provider.DocumentStore.OpenSession();
            var results = from user in session.Query<User>()
                          where user.Username == "test"
                          select user;
            Assert.AreEqual("Is this for real?", results.ToArray()[0].PasswordQuestion);
            Assert.AreEqual("no", results.ToArray()[0].PasswordAnswer);
        }

        [TestMethod]
        public void ShouldKnowHowToStorePasswordQuestionAndAnswerWhenCreating()
        {
            MembershipCreateStatus status;
            provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            var session = provider.DocumentStore.OpenSession();
            var results = from user in session.Query<User>()
                          where user.Username == "test"
                          select user;
            Assert.AreEqual("Is this a test?", results.ToArray()[0].PasswordQuestion);
            Assert.AreEqual("yes", results.ToArray()[0].PasswordAnswer);
        }

        [TestMethod]
        public void ShouldKnowWeDoNotSupportPasswordRetrival()
        {
            Assert.IsFalse(provider.EnablePasswordRetrieval);
        }

        [TestMethod]
        public void ShouldKnowWeSupportPasswordReset()
        {
            Assert.IsTrue(provider.EnablePasswordReset);
        }

        [TestMethod]
        public void ShouldKnowHowToFindUsersByEmail()
        {
            MembershipCreateStatus status;
            var createdUser = provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            int totalRecords;
            var foundUsers = provider.FindUsersByEmail("derp@herp.com", 1, 1, out totalRecords);

            var users = foundUsers.GetEnumerator();
            users.MoveNext();
            Assert.AreEqual(users.Current, createdUser);

        }

        [TestMethod]
        public void ShouldKnowHowToFindUsersByName()
        {
            MembershipCreateStatus status;
            var createdUser = provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            int totalRecords;
            var foundUsers = provider.FindUsersByName("test", 1, 1, out totalRecords);

            var users = foundUsers.GetEnumerator();
            users.MoveNext();
            Assert.AreEqual(users.Current, createdUser);  
        }

        [TestMethod]
        public void ShouldKnowHowToGetPaginatedUsers()
        {
            MembershipCreateStatus status;
            var createdUser = provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            var secondCreatedUser = provider.CreateUser("test2", "password", "derp2@herp.com", "Is this a test?", "yes", true, null, out status);
            int totalRecords;
            var foundUsers = provider.GetAllUsers(1, 1, out totalRecords);
            Assert.AreEqual(2, totalRecords);
            var users = foundUsers.GetEnumerator();
            users.MoveNext();
            Assert.AreEqual(users.Current, createdUser);
            foundUsers = provider.GetAllUsers(2, 1, out totalRecords);
            Assert.AreEqual(2, totalRecords);
            users = foundUsers.GetEnumerator();
            users.MoveNext();
            Assert.AreEqual(users.Current, secondCreatedUser);
        }

        [TestMethod]
        public void ShouldNotKnowHowToGetUsersPassword()
        {
            var retrievedPassword = provider.GetPassword("test", "yes");
            Assert.AreEqual(null, retrievedPassword);
        }

        [TestMethod]
        public void ShouldKnowHowToResetPasswordUsingQuestionAndAnswer()
        {
            MembershipCreateStatus status;
            provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            Thread.Sleep(500);
            var password = provider.ResetPassword("test", "yes");
            Thread.Sleep(500);
            Assert.IsTrue(provider.ValidateUser("test", password));
        }

        [TestMethod]
        [ExpectedException(typeof(MembershipPasswordException), "Expected MembershipPasswordException")]
        public void ShouldKnowNotToResetPasswordWhenAnswerIsWrong()
        {
            MembershipCreateStatus status;
            provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            provider.ResetPassword("test", "no");
        }

        [TestMethod]
        public void ShouldKnowMaxInvalidPasswordAttempts()
        {
            Assert.AreEqual(5, provider.MaxInvalidPasswordAttempts);
        }

        [TestMethod]
        public void ShouldKnowMinRequiredNonAlphanumericCharacters()
        {
            Assert.AreEqual(0, provider.MinRequiredNonAlphanumericCharacters);
        }

        [TestMethod]
        public void ShouldKnowMinRequiredPasswordLength()
        {
            Assert.AreEqual(6, provider.MinRequiredPasswordLength);
        }

        [TestMethod]
        public void ShouldKnowPasswordAttemptWindow()
        {
            Assert.AreEqual(1, provider.PasswordAttemptWindow);
        }

        [TestMethod]
        public void ShouldKnowPasswordFormat()
        {
            Assert.AreEqual(MembershipPasswordFormat.Hashed, provider.PasswordFormat);
        }

        [TestMethod]
        public void ShouldKnowPasswordStrengthRegularExpression()
        {
            Assert.AreEqual("[\\d\\w].*", provider.PasswordStrengthRegularExpression);
        }

        [TestMethod]
        public void ShouldKnowRequiresQuestionAndAnswer()
        {
            Assert.AreEqual(true, provider.RequiresQuestionAndAnswer);
        }

        [TestMethod]
        public void ShouldKnowRequiresUniqueEmail()
        {
            Assert.AreEqual(true, provider.RequiresUniqueEmail);
        }

        [TestMethod]
        public void ShouldKnowAboutLastActivity()
        {
            MembershipCreateStatus status;
            var createdUser = provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            var activeUser = provider.GetUser("test", true);
            Assert.AreNotEqual(createdUser.LastActivityDate, activeUser.LastActivityDate);
        }

        [TestMethod]
        public void ShouldKnowAboutLastLogin()
        {
            MembershipCreateStatus status;
            var createdUser = provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            provider.ValidateUser("test", "password");
            var loggedInUser = provider.GetUser("test", false);
            Assert.AreNotEqual(createdUser.LastLoginDate, loggedInUser.LastLoginDate);
        }

        [TestMethod]
        public void ShouldKnowAboutNumberOfOnlineUsers()
        {
            MembershipCreateStatus status;
            provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            provider.ValidateUser("test", "password");
            Assert.AreEqual(1, provider.GetNumberOfUsersOnline());
            provider.CreateUser("test2", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            Thread.Sleep(500);
            provider.GetUser("test2", true);
            Thread.Sleep(500);
            Assert.AreEqual(2, provider.GetNumberOfUsersOnline());
        }

        [TestMethod]
        public void ShouldKnowHowToUpdateUser()
        {
            MembershipCreateStatus status;
            var createdUser = provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            var newEmail = createdUser.Email = "derp2@herp.com";
            var isApproved = createdUser.IsApproved = false;
            var dateTime = DateTime.Now;
            createdUser.LastLoginDate = dateTime;
            createdUser.LastActivityDate = dateTime;
            var comment = createdUser.Comment = "derp";
            provider.UpdateUser(createdUser);
            var updatedUser = provider.GetUser("test", false);
            Assert.AreEqual(newEmail, updatedUser.Email);
            Assert.AreEqual(isApproved, updatedUser.IsApproved);
            Assert.AreEqual(dateTime, updatedUser.LastLoginDate);
            Assert.AreEqual(dateTime, updatedUser.LastActivityDate);
            Assert.AreEqual(comment, updatedUser.Comment);
        }

        [TestMethod]
        public void ShouldKnowHowToUnlockUser()
        {
            Assert.IsTrue(provider.UnlockUser("bob"));
        }
    }
}
