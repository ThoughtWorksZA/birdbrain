using System;
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
    }
}
