using System;
using System.Web.Security;

namespace BirdBrain
{
    public class BirdBrainMembershipUser : MembershipUser
    {
        public BirdBrainMembershipUser(string name, object providerUserKey, string email, string passwordQuestion,
            string comment, bool isApproved, bool isLockedOut, DateTime creationDate, DateTime lastLoginDate,
            DateTime lastActivityDate, DateTime lastPasswordChangedDate, DateTime lastLockoutDate) 
            : base(BirdBrainMembershipProvider.ProviderName, name, providerUserKey, email, passwordQuestion, comment, isApproved, 
                   isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, lastLockoutDate)
        {
        }

        public BirdBrainMembershipUser(User user)
            : base(
                BirdBrainMembershipProvider.ProviderName, user.Username, user.Id, user.Email, user.PasswordQuestion, user.Comment,
                user.IsApproved, false,
                user.Created, user.LastLogin, user.LastActive, user.LastPasswordChange, user.LastLockedOut)
        {
        }

        protected BirdBrainMembershipUser()
        {
        }

        protected bool Equals(BirdBrainMembershipUser other)
        {
            try
            {
                return ProviderUserKey.Equals(other.ProviderUserKey) && UserName.Equals(other.UserName) &&
                       Email.Equals(other.Email);
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((BirdBrainMembershipUser) obj);
        }

        public override int GetHashCode()
        {
            try
            {
                return ProviderUserKey.GetHashCode() * UserName.GetHashCode() * Email.GetHashCode();
            }
            catch (NullReferenceException)
            {
                return 1337 * 42;
            }
        }
    }
}
