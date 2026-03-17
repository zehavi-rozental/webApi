namespace MyMiddleware.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Display name for JWT
        public string Email { get; set; } = string.Empty; // Unique identifier for login
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // Admin or User
    }
}
