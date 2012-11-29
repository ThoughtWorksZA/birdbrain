using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using Raven.Client.Document;
using Raven.Client.UniqueConstraints;

namespace BirdBrain
{
    public class BirdBrainRoleProvider : RoleProvider
    {
        private DocumentStore documentStore;

        public override string ApplicationName { get; set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            using (var session = documentStore.OpenSession())
            {
                var users = from _user in session.Query<User>()
                            where _user.Username == username
                            select _user;
                var user = users.ToArray().First();
                return user.Roles.Contains(roleName);
            }
        }

        public override string[] GetRolesForUser(string username)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

    }
}
