using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Web.Security;
using Raven.Client;
using Raven.Client.Document;
using WebMatrix.WebData;

namespace BirdBrain
{
    public class BirdBrainExtendedMembershipProvider : ExtendedMembershipProvider
    {
        #region delegation
        protected internal BirdBrainMembershipProvider DelegateProvider { get; set; }

        public BirdBrainExtendedMembershipProvider()
        {
            DelegateProvider = new BirdBrainMembershipProvider();
        }

        protected internal BirdBrainExtendedMembershipProvider(BirdBrainMembershipProvider deletegateProvider)
        {
            DelegateProvider = deletegateProvider;
        }

        public override string ApplicationName
        {
            get { return DelegateProvider.ApplicationName; }
            set { DelegateProvider.ApplicationName = value; }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            DelegateProvider.InitializeBirdBrain(config);
        }

        public void Dispose()
        {
            DelegateProvider.Dispose();
        }

        public DocumentStore DocumentStore
        {
            get { return DelegateProvider.DocumentStore; }
            set { DelegateProvider.DocumentStore = value; }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            return DelegateProvider.ChangePassword(username, oldPassword, newPassword);
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion,
                                                    string newPasswordAnswer)
        {
            return DelegateProvider.ChangePasswordQuestionAndAnswer(username, password, newPasswordQuestion, newPasswordAnswer);
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer,
                                         bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            return DelegateProvider.CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            return DelegateProvider.DeleteUser(username, deleteAllRelatedData);
        }

        public override bool EnablePasswordReset
        {
            get { return DelegateProvider.EnablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return DelegateProvider.EnablePasswordRetrieval; }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return DelegateProvider.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return DelegateProvider.FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            return DelegateProvider.GetAllUsers(pageIndex, pageSize, out totalRecords);
        }

        public override int GetNumberOfUsersOnline()
        {
            return DelegateProvider.GetNumberOfUsersOnline();
        }

        public override string GetPassword(string username, string answer)
        {
            return DelegateProvider.GetPassword(username, answer);
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return DelegateProvider.GetUser(username, userIsOnline);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            return DelegateProvider.GetUser(providerUserKey, userIsOnline);
        }

        public override string GetUserNameByEmail(string email)
        {
            return DelegateProvider.GetUserNameByEmail(email);
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return DelegateProvider.MaxInvalidPasswordAttempts; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return DelegateProvider.MinRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return DelegateProvider.MinRequiredPasswordLength; }
        }

        public override int PasswordAttemptWindow
        {
            get { return DelegateProvider.PasswordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return DelegateProvider.PasswordFormat; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return DelegateProvider.PasswordStrengthRegularExpression; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return DelegateProvider.RequiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return DelegateProvider.RequiresUniqueEmail; }
        }

        public override string ResetPassword(string username, string answer)
        {
            return DelegateProvider.ResetPassword(username, answer);
        }

        public override bool UnlockUser(string userName)
        {
            return DelegateProvider.UnlockUser(userName);
        }

        public override void UpdateUser(MembershipUser user)
        {
            DelegateProvider.UpdateUser(user);
        }

        public override bool ValidateUser(string username, string password)
        {
            return DelegateProvider.ValidateUser(username, password);
        }
#endregion delegation
        public override ICollection<OAuthAccountData> GetAccountsForUser(string userName)
        {
            return new Collection<OAuthAccountData>();
        }

        public override string CreateUserAndAccount(string userName, string password, bool requireConfirmation, IDictionary<string, object> values)
        {
            return CreateAccount(userName, password, requireConfirmation);
        }

        public override string CreateAccount(string userName, string password, bool requireConfirmationToken)
        {

            MembershipCreateStatus status;
            CreateUser(userName, password, null, null, null, !requireConfirmationToken, null, out status);
            if (status.Equals(MembershipCreateStatus.Success) && requireConfirmationToken)
            {
                using (var session = DocumentStore.OpenSession())
                {
                    var user = BirdBrainHelper.GetUserByUsername(userName, session);
                    user.ConfirmationToken = Guid.NewGuid().ToString();
                    session.SaveChanges();
                    return user.ConfirmationToken;
                }
            }
            return null;
        }

        public override bool ConfirmAccount(string userName, string accountConfirmationToken)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var user = BirdBrainHelper.GetUserByUsername(userName, session);
                return ConfirmAccount(accountConfirmationToken, user, session);
            }
        }

        public override bool ConfirmAccount(string accountConfirmationToken)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var user = BirdBrainHelper.GetUserByConfirmationToken(accountConfirmationToken, session);
                return ConfirmAccount(accountConfirmationToken, user, session);
            }
        }

        private static bool ConfirmAccount(string accountConfirmationToken, User user, IDocumentSession session)
        {
            if (user == null)
            {
                return false;
            }
            user.IsApproved = user.ConfirmationToken == accountConfirmationToken;
            session.SaveChanges();
            return user.IsApproved;
        }

        public override bool DeleteAccount(string userName)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var user = BirdBrainHelper.GetUserByUsername(userName, session);
                if (user != null)
                {
                    session.Delete(user);
                    session.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public override string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var user = BirdBrainHelper.GetUserByUsername(userName, session);
                if (user != null)
                {
                    user.PasswordResetToken = Guid.NewGuid().ToString();
                    user.PasswordResetTokenExpiry = DateTime.Now.Add(TimeSpan.FromMinutes(tokenExpirationInMinutesFromNow));
                    session.SaveChanges();
                    return user.PasswordResetToken;
                }
                return null;
            }
        }

        public override int GetUserIdFromPasswordResetToken(string token)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var user = BirdBrainHelper.GetUserByPasswordResetToken(token, session);
                return user != null ? user.GetIdAsInt() : -1;
            }
        }

        public override bool IsConfirmed(string userName)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var user = BirdBrainHelper.GetUserByUsername(userName, session);
                return user != null && user.IsApproved;
            }
        }

        public override bool ResetPasswordWithToken(string token, string newPassword)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var user = BirdBrainHelper.GetUserByPasswordResetToken(token, session);
                if (user != null)
                {
                    user.Password = BirdBrainMembershipProvider.HashPassword(newPassword);
                    user.PasswordResetTokenExpiry = DateTime.MinValue;
                    user.LastPasswordChange = DateTime.Now;
                    session.SaveChanges();
                    return true;
                }
                return false;

            }
        }

        public override int GetPasswordFailuresSinceLastSuccess(string userName)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var user = BirdBrainHelper.GetUserByUsername(userName, session);
                return user != null ? user.PasswordFailuresSinceLastSuccess : 0;
            }
        }

        public override DateTime GetCreateDate(string userName)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var user = BirdBrainHelper.GetUserByUsername(userName, session);
                return user != null ? user.Created : DateTime.MinValue;
            }
        }

        public override DateTime GetPasswordChangedDate(string userName)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var user = BirdBrainHelper.GetUserByUsername(userName, session);
                return user != null ? user.LastPasswordChange : DateTime.MinValue;
            }
        }

        public override DateTime GetLastPasswordFailureDate(string userName)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var user = BirdBrainHelper.GetUserByUsername(userName, session);
                return user != null ? user.LastPasswordFailures : DateTime.MinValue;
            }
        }
    }
}