namespace Infrastructure.EventStore.Abstractions
{
    public interface IUserCredentials
    {
        string Username { get; }
        string Password { get; }
    }
}