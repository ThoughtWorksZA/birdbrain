using System;
using BirdBrain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdBrainTest
{
    [TestClass]
    public class MembershipUserTest
    {
        [TestMethod]
        public void ShouldKnowDifferentUsersMayBeEqual()
        {
            var user = new BirdBrainMembershipUser("BirdBrainMembership", "test", "1", "derp@herp.com", "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            var otherUser = new BirdBrainMembershipUser("BirdBrainMembership", "test", "1", "derp@herp.com", "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            Assert.AreEqual(user, otherUser);
        }

        [TestMethod]
        public void ShouldKnowDifferentUsersMayNotBeEqual()
        {
            var user = new BirdBrainMembershipUser("BirdBrainMembership", "test", "1", "derp@herp.com", "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            var otherUser = new BirdBrainMembershipUser("BirdBrainMembership", "othertest", "1", "derp@herp.com", "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            Assert.AreNotEqual(user, otherUser);
        }

        [TestMethod]
        public void ShouldKnowUsersWithNullFieldsAreNotEqual()
        {
            var user = new BirdBrainMembershipUser("BirdBrainMembership", null, null, null, "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            var otherUser = new BirdBrainMembershipUser("BirdBrainMembership", "othertest", "1", "derp@herp.com", "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            Assert.AreNotEqual(user, otherUser);
        }

        [TestMethod]
        public void ShouldKnowEqualUsersHaveEqualHashCodes()
        {
            var user = new BirdBrainMembershipUser("BirdBrainMembership", "test", "1", "derp@herp.com", "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            var otherUser = new BirdBrainMembershipUser("BirdBrainMembership", "test", "1", "derp@herp.com", "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            Assert.AreEqual(user, otherUser);
            Assert.AreEqual(user.GetHashCode(), otherUser.GetHashCode());
        }

        [TestMethod]
        public void ShouldKnowUnequalUsersMayHaveUnequalHashCodes()
        {
            var user = new BirdBrainMembershipUser("BirdBrainMembership", "test", "1", "derp@herp.com", "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            var otherUser = new BirdBrainMembershipUser("BirdBrainMembership", "othertest", "1", "derp@herp.com", "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            Assert.AreNotEqual(user, otherUser);
            Assert.AreNotEqual(user.GetHashCode(), otherUser.GetHashCode());
        }

        [TestMethod]
        public void ShouldKnowUsersWithNullFieldsHaveEqualHashCodes()
        {
            var user = new BirdBrainMembershipUser("BirdBrainMembership", "test", null, "derp@herp.com", "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            var otherUser = new BirdBrainMembershipUser("BirdBrainMembership", "othertest", "1", null, "", "", true, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            Assert.AreNotEqual(user, otherUser);
            Assert.AreEqual(user.GetHashCode(), otherUser.GetHashCode());
        }
    }
}
