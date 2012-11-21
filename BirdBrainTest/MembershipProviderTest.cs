using System;
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

        [TestInitialize]
        public void Setup()
        {
            var serviceLocator = new TestServiceLocator();
            if (serviceLocator.GetInstance<DocumentStore>() == null)
            {
                var documentStore = new EmbeddableDocumentStore {DataDirectory = "BirdBrainTest"};
                documentStore.Initialize();
                serviceLocator.DoSetDefaultInstance(typeof(DocumentStore), documentStore);
            }
            ServiceLocator.SetLocatorProvider(() => serviceLocator);
            provider = new BirdBrainMembershipProvider();
        }

        [TestMethod]
        public void ShouldKnowHowToCreateUser()
        {
            MembershipCreateStatus status;
            MembershipUser user = provider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            Assert.AreEqual(MembershipCreateStatus.Success, status);
        }

        [TestMethod]
        public void ShouldKnowHowToChangePassword()
        {
            Assert.IsTrue(provider.ChangePassword("derp", "derp", "herp"));
        }
    }
}
