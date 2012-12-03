using System;
using System.Configuration;
using System.Linq;
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
    }
}
