using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace BirdBrain
{
    public class BirdBrainMembershipUser : MembershipUser
    {
        public BirdBrainMembershipUser(string providerName, string name, object providerUserKey, string email, string passwordQuestion,
            string comment, bool isApproved, bool isLockedOut, DateTime creationDate, DateTime lastLoginDate,
            DateTime lastActivityDate, DateTime lastPasswordChangedDate, DateTime lastLockoutDate) 
            : base(providerName, name, providerUserKey, email, passwordQuestion, comment, isApproved, 
                   isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, lastLockoutDate)
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
