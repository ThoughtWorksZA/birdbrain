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
            return users.Count() != 0 ? users.First() : null;
        }

        public static User GetUserByUsername(string username, IDocumentSession session)
        {
            var usersQuery = from user in session.Query<User>()
                             where user.Username == username
                             select user;
            var users = usersQuery.ToArray();
            return users.Count() != 0 ? users.First() : null;
        }

        public static User GetUserByUsernameAndAnswer(string username, string answer, IDocumentSession session)
        {
            var usersQuery = from user in session.Query<User>()
                             where user.Username == username &&
                                   user.PasswordAnswer == answer
                             select user;
            var users = usersQuery.ToArray();
            return users.Count() != 0 ? users.First() : null;
        }
    }
}