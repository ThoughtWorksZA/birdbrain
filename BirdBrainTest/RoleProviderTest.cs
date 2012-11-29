using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Bundles.Encryption.Settings;
using Raven.Bundles.UniqueConstraints;
using Raven.Client.Document;
using Raven.Client.Embedded;
using BirdBrain;
using Raven.Client.Linq;
using Raven.Client.UniqueConstraints;
using Raven.Database.Server;
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
                var documentStore = new EmbeddableDocumentStore
                {
                    RunInMemory = true,
                    UseEmbeddedHttpServer = true,
//                    Configuration =
//                    {
//                        PluginsDirectory = Path.GetDirectoryName(typeof(UniqueConstraintsPutTrigger).Assembly.Location),
//                    }
                };
//                documentStore.Configuration.Settings["Raven/Encryption/Key"] = "ausdj1g2PhUjtSWx6fa+wQzBM1Vf0X8KQCj6tlIq4cU=";
//                documentStore.Configuration.Settings["Raven/ActiveBundles"] = "UniqueConstraints";
                NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8080);
                documentStore.RegisterListener(new UniqueConstraintsStoreListener());
                documentStore.Initialize();
                serviceLocator.DoSetDefaultInstance(typeof(DocumentStore), documentStore);
            }
            ServiceLocator.SetLocatorProvider(() => serviceLocator);
            membershipProvider = new BirdBrainMembershipProvider();
            provider = new BirdBrainRoleProvider();
            var section = (MembershipSection)ConfigurationManager.GetSection("system.web/membership");
            var config = section.Providers["BirdBrainMembership"].Parameters;
            membershipProvider.Initialize("MyApp", config);
            provider.Initialize("MyApp", config);

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
        [ExpectedException(typeof (ProviderException), "Roles cannot be duplicate.")]
        public void CreateRoleShouldThrowProviderExceptionWhenRoleNameIsDuplicate()
        {
            provider.CreateRole("role 1");
            provider.CreateRole("role 1");
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
            provider.AddUsersToRoles(new string[] {"test"}, new string[] {"role 1"});
            var documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
            using (var session = documentStore.OpenSession())
            {
                var users = from user in session.Query<User>()
                            where user.Username == "test"
                            select user;
                var _array = users.ToArray();
                Assert.AreEqual("role 1", _array.First().Roles[0]);
            }
        }

        [TestMethod]
        public void ShouldKnowUserIsInRole()
        {
            MembershipCreateStatus status;
            membershipProvider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            provider.CreateRole("role 1");
            provider.AddUsersToRoles(new string[] { "test" }, new string[] { "role 1" });
            Assert.IsTrue(provider.IsUserInRole("test", "role 1"));
        }

        [TestMethod]
        public void ShouldKnowHowToGetRolesForAUser()
        {
            MembershipCreateStatus status;
            membershipProvider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            var roles = new string[] {"role 1", "role 2"};
            provider.CreateRole(roles[0]);
            provider.CreateRole(roles[1]);
            provider.AddUsersToRoles(new string[] { "test" }, roles);
            var userRoles = provider.GetRolesForUser("test");
            foreach (string role in roles)
            {
                Assert.IsTrue(userRoles.Contains(role), string.Format("User roles [{0}] should contain the role [{1}].", string.Join(", ", userRoles), role));
            }
        }

        [TestMethod]
        public void ShouldKnowHowToGetUsersInRole()
        {
            MembershipCreateStatus status;
            membershipProvider.CreateUser("test", "password", "derp@herp.com", "Is this a test?", "yes", true, null, out status);
            membershipProvider.CreateUser("real", "anotherpassword", "err@herp.com", "What Is that?", "Derp", true, null, out status);
            const string role = "role 1";
            provider.CreateRole(role);
            var expectedUsers = new string[] {"test", "real"};
            provider.AddUsersToRoles(expectedUsers, new string[] {role});

            var usersInARole = provider.GetUsersInRole(role);

            foreach (var user in expectedUsers)
            {
                Assert.IsTrue(usersInARole.Contains(user), string.Format("Role [{0}] should contain the users [{1}].", role, string.Join(", ", expectedUsers)));
            }
        }
    }
}
