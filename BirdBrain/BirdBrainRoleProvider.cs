using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using NLog;
using Raven.Client;
using Raven.Client.Document;

namespace BirdBrain
{
    public class BirdBrainRoleProvider : RoleProvider
    {
        private readonly string providerName = "BirdBrainMembership";

        private readonly DocumentStore documentStore;

        private readonly Logger logger;

        public override string ApplicationName { get; set; }

        public BirdBrainRoleProvider()
        {
            logger = LogManager.GetLogger(typeof(BirdBrainRoleProvider).Name);
            documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
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
//                var roles = from role1 in session.Query<Role>()
//                            where role1.Name == "role 1"
//                            select role1;
//                Console.WriteLine("roles.any?");
//                var rolesList = roles.ToList();
//                if (rolesList.Count() == 0)
//                {
                    Console.WriteLine("saving role");
                    session.Store(role);
                    session.SaveChanges();
//                }
            }
        }

        public void dostuff()
        {
            using (var session = documentStore.OpenSession())
            {
                var results = session.Query<Role>()
                                     .Where(x => x.Name == "role1")
                                     .ToList();
                if (!results.Any())
                {
                    session.Store(new Role("role1"));
                    session.SaveChanges();
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
//            Console.WriteLine(roleNames.First());
//
//            using (var session = documentStore.OpenSession())
//            {
//                foreach (var username in usernames)
//                {
//                    var user1 = (from user in session.Query<User>()
//                                 where user.Username == username
//                                 select user
//                                 ).ToArray().First();
//                    if (user1.Roles == null)
//                    {
//                        user1.Roles = new string[] {};
//                    }
//                    user1.Roles = user1.Roles.Concat(roleNames).ToArray();
//                    
//                    Console.WriteLine(roleNames.First());
//                    Console.WriteLine(user1.Roles);
//                    session.Store(user1);
//                    session.SaveChanges();
//                }
//            }
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
