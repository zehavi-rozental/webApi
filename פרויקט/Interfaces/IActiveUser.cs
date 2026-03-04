namespace Shared.Interfaces
{
    public interface IActiveUser
    {
        ActiveUserData? ActiveUser { get; }
    }

    public class ActiveUserData
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // Default role
    }
}
