using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Linq;
using System.Threading;
using System.Web.Security;
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
        private MembershipProvider membershipProvider;
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
            membershipProvider = new BirdBrainMembershipProvider();
            membershipProvider.Initialize("MyApp", new NameValueCollection(ConfigurationManager.AppSettings));

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
        [ExpectedException(typeof (ProviderException), "Role name already exists")]
        public void CreateRoleShouldThrowProviderExceptionWhenRoleNameIsDuplicate()
        {
            provider.CreateRole("role 1");
            var documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
            using (var session = documentStore.OpenSession())
            {
                var roles = from role1 in session.Query<Role>()
                            where role1.Name == "role 1"
                            select role1;
                Console.WriteLine("roles.any?");
                if (roles.Any())
                {
                    Console.WriteLine("yes there are roles");
                }
            }
            provider.CreateRole("role 1");
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException), "Role name already exists")]
        public void CreateRoleShouldThrowProviderExceptionWhenRoleNameIsDuplicate1()
        {
            provider.dostuff();
            Thread.Sleep(2000);
            provider.dostuff();
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Username can not be null")]
        public void AddUsersToRolesShouldThrowArgumentNullExceptionWhenAnyOfUserNamesIsNull()
        {
            var userNames = new string[] {"user 1", "user 2", null};
            var roleNames = new string[] {"role 1", "role 2"};
            provider.AddUsersToRoles(userNames, roleNames);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Username can not be empty")]
        public void AddUsersToRolesShouldThrowArgumentExceptionWhenAnyOfUserNamesIsEmpty()
        {
            var userNames = new string[] { "user 1", "user 2", "" };
            var roleNames = new string[] { "role 1", "role 2" };
            provider.AddUsersToRoles(userNames, roleNames);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Role name can not be null")]
        public void AddUsersToRolesShouldThrowArgumentNullExceptionWhenAnyOfRoleNamesIsNull()
        {
            var userNames = new string[] { "user 1", "user 2", "user 3" };
            var roleNames = new string[] { "role 1", null };
            provider.AddUsersToRoles(userNames, roleNames);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Role name can not be empty")]
        public void AddUsersToRolesShouldThrowArgumentExceptionWhenAnyOfRoleNamesIsEmpty()
        {
            var userNames = new string[] { "user 1", "user 2", "user 3" };
            var roleNames = new string[] { "role 1", "" };
            provider.AddUsersToRoles(userNames, roleNames);
        }

        [TestMethod]
        public void AddUsersToRolesShouldAssociateTheRolesToTheUsers()
        {
            MembershipCreateStatus status;
            membershipProvider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            provider.CreateRole("role 1");
            var documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
            using (var session = documentStore.OpenSession())
            {

                var roles = from role in session.Query<Role>()
                            where role.Name == "role 1"
                            select role;
                var role1 = roles.ToArray().First();
                var users = from user in session.Query<User>()
                            where user.Username == "test"
                            select user;
                var user1 = users.ToArray().First();
                var userNames = new string[] {user1.Username};
                var roleNames = new string[] {role1.Name};

                Assert.IsFalse(user1.Roles.Any());

                provider.AddUsersToRoles(userNames, roleNames);
                users = from user in session.Query<User>()
                            where user.Username == "test"
                            select user;
                user1 = users.ToArray().First();
                
                Assert.AreEqual("role 1", user1.Roles.First());
            }
        }
    }
}
