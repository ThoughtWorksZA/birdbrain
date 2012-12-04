using System;

namespace BirdBrain
{
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public string[] Roles { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime LastActive { get; set; }
        public DateTime LastPasswordChange { get; set; }
        public DateTime LastLockedOut { get; set; }
        public bool IsApproved { get; set; }
        public string Comment { get; set; }
        public string ConfirmationToken { get; set; }
        public string PasswordResetToken { get; set; }
        public DateTime PasswordResetTokenExpiry { get; set; }
        public int PasswordFailuresSinceLastSuccess { get; set; }
        public DateTime LastPasswordFailures { get; set; }

        public User(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved)
        {
            Username = username;
            Password = password;
            Email = email;
            PasswordQuestion = passwordQuestion;
            PasswordAnswer = passwordAnswer;
            IsApproved = isApproved;
            Created = DateTime.Now;
            LastLogin = DateTime.MinValue;
            LastActive = DateTime.MinValue;
            LastPasswordChange = DateTime.MinValue;
            LastLockedOut = DateTime.MinValue;
            Comment = "";
            Roles = new string[] {};
        }

        public User()
        {
        }

        public int GetIdAsInt()
        {
            return Int32.Parse(Id.Split('/')[1]);
        }
    }
}