using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Security;
using Raven.Client.Document;

namespace BirdBrain
{
    public class BirdBrainRoleProvider : RoleProvider
    {
        private string providerName;
        public override string Name
        {
            get { return providerName; }
        }

        private DocumentStore documentStore;
        public static readonly string ConnectionStringName = "BirdBrain";

        public override string ApplicationName { get; set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            providerName = name;
            base.Initialize(name, config);
            InitializeDocumentStore();
        }

        public void Dispose()
        {
            documentStore.Dispose();
        }

        public DocumentStore DocumentStore
        {
            get { return documentStore; }
            set
            {
                documentStore = value;
            }
        }

        protected virtual void InitializeDocumentStore()
        {
            DocumentStore = new DocumentStore
            {
                ConnectionStringName = BirdBrainRoleProvider.ConnectionStringName,
            };
            DocumentStore.Initialize();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            using (var session = documentStore.OpenSession())
            {
                var user = BirdBrainHelper.GetUserByUsername(username, session);
                return user.Roles.Contains(roleName);
            }
        }

        public override string[] GetRolesForUser(string username)
        {
            using (var session = documentStore.OpenSession())
            {
                var user = BirdBrainHelper.GetUserByUsername(username, session);
                return user != null ? user.Roles : new string[] {};
            }
        }

        public override void CreateRole(string roleName)
        {
            if (roleName == null)
            {
                throw new ArgumentNullException("Role name can not be null");
            }
            if (roleName == "")
            {
                throw new ArgumentException("Role name can not be empty");
            }
            if (roleName.Contains(","))
            {
                throw new ArgumentException("Role name can not have a comma in it");
            }
            
            var role = new Role(roleName);
            using (var session = documentStore.OpenSession())
            {
                session.Advanced.UseOptimisticConcurrency = true;
                try
                {
                    session.Store(role);
                    session.SaveChanges();
                }
                catch
                {
                    throw new ProviderException("The role already exists.");
                }
            }
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (!RoleExists(roleName))
            {
                throw new ArgumentException(string.Format("Role [{0}] does not exist.", roleName));
            }
            using (var session = documentStore.OpenSession())
            {
                var usersQuery = from _user in session.Query<User>()
                                 where _user.Roles.Contains(roleName)
                                 select _user;
                var users = usersQuery.ToArray();
                if (users.Any() && !throwOnPopulatedRole)
                {
                    foreach (var user in users)
                    {
                        var roles = user.Roles.ToList();
                        roles.Remove(roleName);
                        user.Roles = roles.ToArray();
                    }
                }
                else
                {
                    throw new ProviderException(string.Format("Role [{0}] is in use and cannot be deleted.", roleName));
                }
                session.Delete(session.Load<Role>(new Role(roleName).Id));
                session.SaveChanges();
                return true;
            }
        }

        public override bool RoleExists(string roleName)
        {
            if (roleName == null)
            {
                throw new ArgumentNullException("Role name can not be null");
            }
            if (roleName == "")
            {
                throw new ArgumentException("Role name can not be empty");
            }
            using (var session = documentStore.OpenSession())
            {
                var roles = from role in session.Query<Role>()
                            where role.Name == roleName
                            select role;
                return roles.Any();
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            if (usernames.Contains(null))
            {
                throw new ArgumentNullException("Username can not be null");
            }
            if (usernames.Contains(""))
            {
                throw new ArgumentException("Username can not be empty");
            }
            if (roleNames.Contains(null))
            {
                throw new ArgumentNullException("Role name can not be null");
            }
            if (roleNames.Contains(""))
            {
                throw new ArgumentException("Role name can not be empty");
            }

            using (var session = documentStore.OpenSession())
            {
                foreach (var username in usernames)
                {
                    var users = from _user in session.Query<User>()
                                where _user.Username == username
                                select _user;
                    var user = users.ToArray().First();
                    user.Roles = user.Roles.Union(roleNames).ToArray();
                    session.Store(user);
                    session.SaveChanges();
                }
            }
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            using (var session = documentStore.OpenSession())
            {
                foreach (var username in usernames)
                {
                    var users = from _user in session.Query<User>()
                                where _user.Username == username
                                select _user;
                    if (users.ToArray().Length == 0)
                    {
                        throw new ProviderException(string.Format("The user [{0}] does not exist.", username));
                    }
                    var user = users.ToArray().First();
                    user.Roles = user.Roles.Except(roleNames).ToArray();
                    session.Store(user);
                }
                session.SaveChanges();
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            using (var session = documentStore.OpenSession())
            {
                var usersQuery = from user in session.Query<User>()
                                 where user.Roles.Contains(roleName)
                                 select user.Username;
                return usersQuery.ToArray();
            }
        }

        public override string[] GetAllRoles()
        {
            var session = documentStore.OpenSession();
            var roleNames = from role in session.Query<Role>()
                        select role.Name;
            return roleNames.ToArray();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            if (!RoleExists(roleName))
            {
                throw new ProviderException("The specified role does not exist.");
            }
            if (GetRolesForUser(usernameToMatch).Contains(roleName))
            {
                return new string[] {usernameToMatch};
            }
            return new string[] {};
        }

    }
}
