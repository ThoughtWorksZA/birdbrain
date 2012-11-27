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

        private static User[] GetUsersByUsernameAndPassword(string username, string password, IDocumentSession session)
        {
            var usersQuery = from user in session.Query<User>()
                          where user.Username == username &&
                                user.Password == password
                          select user;
            var users = usersQuery.ToArray();
            return users;
        }

        private static User[] GetUsersByUsername(string username, IDocumentSession session)
        {
            var usersQuery = from user in session.Query<User>()
                          where user.Username == username
                          select user;
            var users = usersQuery.ToArray();
            return users;
        }

        public override string ApplicationName { get; set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
            base.Initialize(name, config);
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            var session = documentStore.OpenSession();
            var users = GetUsersByUsernameAndPassword(username, oldPassword, session);
            if (users.Count() == 1)
            {
                users[0].Password = newPassword;
                session.Store(users[0]);
                session.SaveChanges();
                return true;
            }
            return false;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            var session = documentStore.OpenSession();
            var users = GetUsersByUsernameAndPassword(username, password, session);
            if (users.Count() == 1)
            {
                users[0].PasswordQuestion = newPasswordQuestion;
                users[0].PasswordAnswer = newPasswordAnswer;
                session.Store(users[0]);
                session.SaveChanges();
                return true;
            }
            return false;
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            var user = new User(username, password, email, passwordQuestion, passwordAnswer);
            var session = documentStore.OpenSession();
            session.Store(user);
            session.SaveChanges();
            status = MembershipCreateStatus.Success;
            return new BirdBrainMembershipUser(ProviderName, user.Username, user.Id, user.Email, passwordQuestion, "", isApproved, false, DateTime.Now, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            var session = documentStore.OpenSession();
            var users = GetUsersByUsername(username, session);
            if (users.Count() == 1)
            {
                session.Delete(users[0]);
                session.SaveChanges();
                return true;
            }
            return false;
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
            var session = documentStore.OpenSession();
            var results = from _user in session.Query<User>()
                          where _user.Email == emailToMatch
                          select _user;
            totalRecords = results.Count();
            var users = new MembershipUserCollection();
            foreach (var user in results)
            {
                users.Add(new BirdBrainMembershipUser(ProviderName, user.Username, user.Id, user.Email, user.PasswordQuestion, "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue));
            }
            return users;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var session = documentStore.OpenSession();
            var results = from _user in session.Query<User>()
                          where _user.Username == usernameToMatch
                          select _user;
            totalRecords = results.Count();
            var users = new MembershipUserCollection();
            foreach (var user in results)
            {
                users.Add(new BirdBrainMembershipUser(ProviderName, user.Username, user.Id, user.Email, user.PasswordQuestion, "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue));
            }
            return users;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
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
            var session = documentStore.OpenSession();
            var user = GetUsersByUsername(username, session).First();
            return new BirdBrainMembershipUser(ProviderName, user.Username, user.Id, user.Email, "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            var session = documentStore.OpenSession();
            var user = session.Load<User>(providerUserKey.ToString());
            return new BirdBrainMembershipUser(ProviderName, user.Username, user.Id, user.Email, "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
        }

        public override string GetUserNameByEmail(string email)
        {
            var session = documentStore.OpenSession();
            var usersQuery = from _user in session.Query<User>()
                          where _user.Email == email
                          select _user.Username;
            return usersQuery.ToArray().First();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {
            get { throw new NotImplementedException(); }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { throw new NotImplementedException(); }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
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
            var session = documentStore.OpenSession();
            var users = GetUsersByUsernameAndPassword(username, password, session);
            return users.Count() != 0;
        }
    }
}
