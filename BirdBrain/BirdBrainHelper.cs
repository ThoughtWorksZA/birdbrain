using System;
using System.Linq;
using Raven.Client;

namespace BirdBrain
{
    internal static class BirdBrainHelper
    {
        public static User GetUserByUsernameAndPassword(string username, string password, IDocumentSession session)
        {
            var usersQuery = from user in session.Query<User>()
                             where user.Username == username &&
                                   user.Password == password
                             select user;
            var users = usersQuery.ToArray();
            return users.Any() ? users.First() : null;
        }

        public static User GetUserByUsername(string username, IDocumentSession session)
        {
            var usersQuery = from user in session.Query<User>()
                             where user.Username == username
                             select user;
            var users = usersQuery.ToArray();
            return users.Any() ? users.First() : null;
        }

        public static User GetUserByUsernameAndAnswer(string username, string answer, IDocumentSession session)
        {
            var usersQuery = from user in session.Query<User>()
                             where user.Username == username &&
                                   user.PasswordAnswer == answer
                             select user;
            var users = usersQuery.ToArray();
            return users.Any() ? users.First() : null;
        }

        public static User GetUserByConfirmationToken(string accountConfirmationToken, IDocumentSession session)
        {
            var usersQuery = from user in session.Query<User>()
                             where user.ConfirmationToken == accountConfirmationToken
                             select user;
            var users = usersQuery.ToArray();
            return users.Any() ? users.First() : null;
        }

        public static User GetUserByPasswordResetToken(string token, IDocumentSession session)
        {
            var usersQuery = from user in session.Query<User>()
                             where user.PasswordResetToken == token &&
                                   DateTime.UtcNow <= user.PasswordResetTokenExpiry
                             select user;
            var users = usersQuery.ToArray();
            return users.Any() ? users.First() : null;
        }

        public static User GetUserById(int id, IDocumentSession session)
        {
            return session.Load<User>(string.Format("users/{0}", id));
        }
    }
}