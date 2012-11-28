using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using Raven.Client;
using Raven.Client.Document;

namespace BirdBrain
{
    public class BirdBrainMembershipProvider : MembershipProvider
    {
        private const string ProviderName = "BirdBrainMembership";

        private DocumentStore documentStore;
        private int minRequiredPasswordLength = 6;
        private int maxInvalidPasswordAttempts = 5;
        private int minRequiredNonAlphanumericCharacters = 0;
        private int passwordAttemptWindow = 1;
        private MembershipPasswordFormat passwordFormat = MembershipPasswordFormat.Clear;
        private string passwordStrengthRegularExpression = "[\\d\\w].*";
        private bool requiresQuestionAndAnswer = true;

        public override string ApplicationName { get; set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            foreach (var key in config.AllKeys)
            {
                Console.Out.WriteLine(key + " - " + config[key]);
            }
            if (config["minRequiredPasswordLength"] != null)
            {
                minRequiredPasswordLength = int.Parse(config["minRequiredPasswordLength"]);
            }
            if (config["maxInvalidPasswordAttempts"] != null)
            {
                maxInvalidPasswordAttempts = int.Parse(config["maxInvalidPasswordAttempts"]);
            }
            if (config["minRequiredNonAlphanumericCharacters"] != null)
            {
                minRequiredNonAlphanumericCharacters = int.Parse(config["minRequiredNonAlphanumericCharacters"]);
            }
            if (config["passwordAttemptWindow"] != null)
            {
                passwordAttemptWindow = int.Parse(config["passwordAttemptWindow"]);
            }
            if (config["passwordFormat"] != null)
            {
                MembershipPasswordFormat _passwordFormat;
                if (MembershipPasswordFormat.TryParse(config["passwordFormat"], true, out _passwordFormat))
                {
                    passwordFormat = _passwordFormat;
                }
            }
            if (config["passwordStrengthRegularExpression"] != null)
            {
                passwordStrengthRegularExpression = config["passwordStrengthRegularExpression"];
            }
            if (config["requiresQuestionAndAnswer"] != null)
            {
                requiresQuestionAndAnswer = bool.Parse(config["requiresQuestionAndAnswer"]);
            }
            documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
        }

        private static User GetUserByUsernameAndPassword(string username, string password, IDocumentSession session)
        {
            var usersQuery = from user in session.Query<User>()
                          where user.Username == username &&
                                user.Password == password
                          select user;
            var users = usersQuery.ToArray();
            return users.Count() != 0 ? users.First() : null;
        }

        private static User GetUserByUsername(string username, IDocumentSession session)
        {
            var usersQuery = from user in session.Query<User>()
                          where user.Username == username
                          select user;
            var users = usersQuery.ToArray();
            return users.Count() != 0 ? users.First() : null;
        }

        private static User GetUserByUsernameAndAnswer(string username, string answer, IDocumentSession session)
        {
            var usersQuery = from user in session.Query<User>()
                             where user.Username == username &&
                                   user.PasswordAnswer == answer
                             select user;
            var users = usersQuery.ToArray();
            return users.Count() != 0 ? users.First() : null;
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            using (var session = documentStore.OpenSession())
            {
                var user = GetUserByUsernameAndPassword(username, oldPassword, session);
                if (user != null)
                {
                    user.Password = newPassword;
                    session.Store(user);
                    session.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            using (var session = documentStore.OpenSession())
            {
                var user = GetUserByUsernameAndPassword(username, password, session);
                if (user != null)
                {
                    user.PasswordQuestion = newPasswordQuestion;
                    user.PasswordAnswer = newPasswordAnswer;
                    session.Store(user);
                    session.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            var user = new User(username, password, email, passwordQuestion, passwordAnswer);
            using (var session = documentStore.OpenSession())
            {
                session.Store(user);
                session.SaveChanges();
                status = MembershipCreateStatus.Success;
                return new BirdBrainMembershipUser(ProviderName, user.Username, user.Id, user.Email, passwordQuestion,
                                                   "", isApproved, false, DateTime.Now, DateTime.MinValue,
                                                   DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            }
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            using (var session = documentStore.OpenSession())
            {
                var user = GetUserByUsername(username, session);
                if (user != null)
                {
                    session.Delete(user);
                    session.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public override bool EnablePasswordReset
        {
            get { return true; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            using (var session = documentStore.OpenSession())
            {
                var results = from _user in session.Query<User>()
                              where _user.Email == emailToMatch
                              select _user;
                totalRecords = results.Count();
                var users = new MembershipUserCollection();
                foreach (var user in results)
                {
                    users.Add(new BirdBrainMembershipUser(ProviderName, user.Username, user.Id, user.Email,
                                                          user.PasswordQuestion, "", true, false, DateTime.MinValue,
                                                          DateTime.MinValue, DateTime.MinValue, DateTime.MinValue,
                                                          DateTime.MinValue));
                }
                return users;
            }
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            using (var session = documentStore.OpenSession())
            {
                var results = from _user in session.Query<User>()
                              where _user.Username == usernameToMatch
                              select _user;
                totalRecords = results.Count();
                var users = new MembershipUserCollection();
                foreach (var user in results)
                {
                    users.Add(new BirdBrainMembershipUser(ProviderName, user.Username, user.Id, user.Email,
                                                          user.PasswordQuestion, "", true, false, DateTime.MinValue,
                                                          DateTime.MinValue, DateTime.MinValue, DateTime.MinValue,
                                                          DateTime.MinValue));
                }
                return users;
            }
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            using (var session = documentStore.OpenSession())
            {
                var results = session.Query<User>().Select(_user => _user).Skip((pageIndex - 1)*pageSize).Take(pageSize);
                totalRecords = results.Count();
                var membershipUsers = new MembershipUserCollection();
                foreach (var user in results)
                {
                    membershipUsers.Add(new BirdBrainMembershipUser(ProviderName, user.Username, user.Id, user.Email,
                                                                    user.PasswordQuestion, "", true, false,
                                                                    DateTime.MinValue, DateTime.MinValue,
                                                                    DateTime.MinValue, DateTime.MinValue,
                                                                    DateTime.MinValue));
                }
                return membershipUsers;
            }
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            return null;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            using (var session = documentStore.OpenSession())
            {
                var user = GetUserByUsername(username, session);
                return new BirdBrainMembershipUser(ProviderName, user.Username, user.Id, user.Email, "", "", true, false,
                                                   DateTime.MinValue, DateTime.MinValue, DateTime.MinValue,
                                                   DateTime.MinValue, DateTime.MinValue);
            }
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            using (var session = documentStore.OpenSession())
            {
                var user = session.Load<User>(providerUserKey.ToString());
                return new BirdBrainMembershipUser(ProviderName, user.Username, user.Id, user.Email, "", "", true, false,
                                                   DateTime.MinValue, DateTime.MinValue, DateTime.MinValue,
                                                   DateTime.MinValue, DateTime.MinValue);
            }
        }

        public override string GetUserNameByEmail(string email)
        {
            using (var session = documentStore.OpenSession())
            {
                var usersQuery = from _user in session.Query<User>()
                                 where _user.Email == email
                                 select _user.Username;
                return usersQuery.ToArray().First();
            }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return maxInvalidPasswordAttempts; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return minRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return minRequiredPasswordLength; }
        }

        public override int PasswordAttemptWindow
        {
            get { return passwordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return passwordFormat; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return passwordStrengthRegularExpression; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return requiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return true; }
        }

        public override string ResetPassword(string username, string answer)
        {
            using (var session = documentStore.OpenSession())
            {
                var user = GetUserByUsernameAndAnswer(username, answer, session);
                if (user != null)
                {
                    var password = Membership.GeneratePassword(MinRequiredPasswordLength, MinRequiredNonAlphanumericCharacters);
                    user.Password = password;
                    session.Store(user);
                    session.SaveChanges();
                    return password;
                }
                throw new MembershipPasswordException("Unable to reset password.");
            }
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            using (var session = documentStore.OpenSession())
            {
                return GetUserByUsernameAndPassword(username, password, session) != null;
            }
        }
    }
}
