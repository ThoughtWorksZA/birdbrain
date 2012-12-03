using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Security;
using Raven.Client.Document;
using WebMatrix.WebData;

namespace BirdBrain
{
    public class BirdBrainExtendedMembershipProvider : ExtendedMembershipProvider
    {
        private BirdBrainMembershipProvider delegateProvider;
        public override string ApplicationName
        {
            get { return delegateProvider.ApplicationName; }
            set { delegateProvider.ApplicationName = value; }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            delegateProvider.Initialize(name, config);
        }

        public void Dispose()
        {
            delegateProvider.Dispose();
        }

        public DocumentStore DocumentStore
        {
            get { return delegateProvider.DocumentStore; }
            set { delegateProvider.DocumentStore = value; }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            return delegateProvider.ChangePassword(username, oldPassword, newPassword);
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion,
                                                    string newPasswordAnswer)
        {
            return delegateProvider.ChangePasswordQuestionAndAnswer(username, password, newPasswordQuestion, newPasswordAnswer);
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer,
                                         bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            return delegateProvider.CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            return delegateProvider.DeleteUser(username, deleteAllRelatedData);
        }

        public override bool EnablePasswordReset
        {
            get { return delegateProvider.EnablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return delegateProvider.EnablePasswordRetrieval; }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return delegateProvider.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return delegateProvider.FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            return delegateProvider.GetAllUsers(pageIndex, pageSize, out totalRecords);
        }

        public override int GetNumberOfUsersOnline()
        {
            return delegateProvider.GetNumberOfUsersOnline();
        }

        public override string GetPassword(string username, string answer)
        {
            return delegateProvider.GetPassword(username, answer);
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return delegateProvider.GetUser(username, userIsOnline);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            return delegateProvider.GetUser(providerUserKey, userIsOnline);
        }

        public override string GetUserNameByEmail(string email)
        {
            return delegateProvider.GetUserNameByEmail(email);
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return delegateProvider.MaxInvalidPasswordAttempts; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return delegateProvider.MinRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return delegateProvider.MinRequiredPasswordLength; }
        }

        public override int PasswordAttemptWindow
        {
            get { return delegateProvider.PasswordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return delegateProvider.PasswordFormat; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return delegateProvider.PasswordStrengthRegularExpression; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return delegateProvider.RequiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return delegateProvider.RequiresUniqueEmail; }
        }

        public override string ResetPassword(string username, string answer)
        {
            return delegateProvider.ResetPassword(username, answer);
        }

        public override bool UnlockUser(string userName)
        {
            return delegateProvider.UnlockUser(userName);
        }

        public override void UpdateUser(MembershipUser user)
        {
            delegateProvider.UpdateUser(user);
        }

        public override bool ValidateUser(string username, string password)
        {
            return delegateProvider.ValidateUser(username, password);
        }

        public override ICollection<OAuthAccountData> GetAccountsForUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override string CreateUserAndAccount(string userName, string password, bool requireConfirmation, IDictionary<string, object> values)
        {
            throw new NotImplementedException();
        }

        public override string CreateAccount(string userName, string password, bool requireConfirmationToken)
        {
            throw new NotImplementedException();
        }

        public override bool ConfirmAccount(string userName, string accountConfirmationToken)
        {
            throw new NotImplementedException();
        }

        public override bool ConfirmAccount(string accountConfirmationToken)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteAccount(string userName)
        {
            throw new NotImplementedException();
        }

        public override string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow)
        {
            throw new NotImplementedException();
        }

        public override int GetUserIdFromPasswordResetToken(string token)
        {
            throw new NotImplementedException();
        }

        public override bool IsConfirmed(string userName)
        {
            throw new NotImplementedException();
        }

        public override bool ResetPasswordWithToken(string token, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override int GetPasswordFailuresSinceLastSuccess(string userName)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetCreateDate(string userName)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetPasswordChangedDate(string userName)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetLastPasswordFailureDate(string userName)
        {
            throw new NotImplementedException();
        }
    }
}