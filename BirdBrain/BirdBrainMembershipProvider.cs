using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using NLog;
using Raven.Client;
using Raven.Client.Document;
using System.Security.Cryptography;

namespace BirdBrain
{
    public class BirdBrainMembershipProvider : MembershipProvider
    {
        private readonly string providerName = "BirdBrainMembership";

        private readonly DocumentStore documentStore;

        private readonly Logger logger;

        public override string ApplicationName { get; set; }

        public BirdBrainMembershipProvider()
        {
            logger = LogManager.GetLogger(typeof (BirdBrainMembershipProvider).Name);
            documentStore = ServiceLocator.Current.GetInstance<DocumentStore>();
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            var session = documentStore.OpenSession();
            var results = from user in session.Query<User>()
                          where user.Username == username && 
                                user.Password == oldPassword
                          select user;
            var users = results.ToArray();
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
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            var user = new User(username, password, email);
            var session = documentStore.OpenSession();
            session.Store(user);
            session.SaveChanges();
            status = MembershipCreateStatus.Success;
            return new BirdBrainMembershipUser(providerName, user.Username, user.Id, user.Email, passwordQuestion, "", isApproved, false, DateTime.Now, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            var session = documentStore.OpenSession();
            var results = from user in session.Query<User>()
                          where user.Username == username
                          select user;
            var users = results.ToArray();
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
            get { throw new NotImplementedException(); }
        }

        public override bool EnablePasswordRetrieval
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            var session = documentStore.OpenSession();
            var results = from _user in session.Query<User>()
                          where _user.Username == username
                          select _user;
            var user = results.ToArray()[0];
            return new BirdBrainMembershipUser(providerName, user.Username, user.Id, user.Email, "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            var session = documentStore.OpenSession();
            var user = session.Load<User>(providerUserKey.ToString());
            return new BirdBrainMembershipUser(providerName, user.Username, user.Id, user.Email, "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
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
            var results = from user in session.Query<User>()
                          where user.Username == username &&
                                user.Password == password
                          select user;
            return results.Count() == 1;
        }
    }
}
