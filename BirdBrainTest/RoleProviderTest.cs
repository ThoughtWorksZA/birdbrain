using System;
using System.Linq;
using ICSharpCode.NRefactory;
using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Client.Document;
using Raven.Client.Embedded;
using BirdBrain;
using Raven.Client.Linq;
using Role = BirdBrain.Role;

namespace BirdBrainTest
{
    [TestClass]
    public class RoleProviderTest
    {
        private BirdBrainRoleProvider provider;
        private TestServiceLocator serviceLocator;

        [TestInitialize]
        public void Setup()
        {
            serviceLocator = new TestServiceLocator();
            if (serviceLocator.GetInstance<DocumentStore>() == null)
            {
                var documentStore = new EmbeddableDocumentStore { RunInMemory = true };
                documentStore.Initialize();
                serviceLocator.DoSetDefaultInstance(typeof(DocumentStore), documentStore);
            }
            ServiceLocator.SetLocatorProvider(() => serviceLocator);
            provider = new BirdBrainRoleProvider();
        }

        [TestCleanup]
        public void Cleanup()
        {
            var documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
            documentStore.Dispose();
            serviceLocator.DoSetClearDefaultInstance(typeof(DocumentStore));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Role name can not be empty")]
        public void CreateRoleShouldThrowArgumentExceptionWhenRoleNameIsEmpty()
        {
            provider.CreateRole("");
        }
 
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Role name can not be null")]
        public void CreateRoleShouldThrowArgumentNullExceptionWhenRoleNameIsNull()
        {
            provider.CreateRole(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Role name can not have a comma in it")]
        public void CreateRoleShouldThrowArgumentNullExceptionWhenRoleNameHasComma()
        {
            provider.CreateRole("hi,there");
        }

        [TestMethod]
        public void CreateRoleShouldCreateUserWhenRoleNameIsValid()
        {
            provider.CreateRole("test role");
            var documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
            var session = documentStore.OpenSession();
            var roles = from role in session.Query<Role>()
                          where role.Name == "test role"
                          select role;
            Assert.AreEqual(1, roles.Count());
        }

        [TestMethod]
        public void GetAllRolesShouldReturnEmptyRoleNamesWhenThereAreNoRoles()
        {
            string[] roleNames = provider.GetAllRoles();
            Assert.AreEqual(0, roleNames.Count());
        }

        [TestMethod]
        public void GetAllRolesShouldReturnListOfRoleNames()
        {
            provider.CreateRole("role 1");
            provider.CreateRole("role 2");
            string[] roleNames = provider.GetAllRoles();
            Assert.AreEqual(2, roleNames.Count());
            Assert.AreEqual("role 1", roleNames[0]);
            Assert.AreEqual("role 2", roleNames[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Role name can not be null")]
        public void RoleExistsShouldThrowArgumentNullExceptionWhenRoleNameIsNull()
        {
            provider.RoleExists(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Role name can not be empty")]
        public void RoleExistsShouldThrowArgumentExceptionWhenRoleNameIsEmpty()
        {
            provider.RoleExists("");
        }

        [TestMethod]
        public void RoleExistsShouldReturnTrueWhenRoleNameExists()
        {
            provider.CreateRole("my new new role");
            Assert.IsTrue(provider.RoleExists("my new new role"));
        }

        [TestMethod]
        public void RoleExistsShouldReturnFalseWhenRoleNameDoesNotExist()
        {
            provider.CreateRole("my role");
            Assert.IsFalse(provider.RoleExists("my role1"));
        }
    }
}
