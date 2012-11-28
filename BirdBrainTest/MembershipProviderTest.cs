using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web.Security;
using BirdBrain;
using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Client.Document;
using Raven.Client.Embedded;

namespace BirdBrainTest
{
    [TestClass]
    public class MembershipProviderTest
    {
        private BirdBrainMembershipProvider provider;
        private TestServiceLocator serviceLocator;

        [TestInitialize]
        public void Setup()
        {
            serviceLocator = new TestServiceLocator();
            if (serviceLocator.GetInstance<DocumentStore>() == null)
            {
                var documentStore = new EmbeddableDocumentStore {RunInMemory = true};
                documentStore.Initialize();
                serviceLocator.DoSetDefaultInstance(typeof(DocumentStore), documentStore);
            }
            ServiceLocator.SetLocatorProvider(() => serviceLocator);
            provider = new BirdBrainMembershipProvider();
            provider.Initialize("MyApp", new NameValueCollection(ConfigurationManager.AppSettings));
        }

        [TestCleanup]
        public void Cleanup()
        {
            var documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
            documentStore.Dispose();
            serviceLocator.DoSetClearDefaultInstance(typeof(DocumentStore));
        }

        [TestMethod]
        public void ShouldKnowHowToCreateUser()
        {
            MembershipCreateStatus status;
            var membershipUser = provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            Assert.AreEqual(MembershipCreateStatus.Success, status);
            Assert.AreEqual("test", membershipUser.UserName);
            var documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
            var session = documentStore.OpenSession();
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
            var createdUser = provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            Assert.IsTrue(provider.DeleteUser("test", false));
            var documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
            var session = documentStore.OpenSession();
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
            var documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
            var session = documentStore.OpenSession();
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
            var documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
            var session = documentStore.OpenSession();
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
    }
}
