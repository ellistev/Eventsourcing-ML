using Infrastructure.EventStore.Abstractions;

namespace Infrastructure.EventStore
{
    public class UserCredentials : IUserCredentials
    {
        public UserCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; }
        public string Password { get; }
    }
}