namespace MyMiddleware.Models
{
    public class IceCream
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Milki { get; set; }
        public string UserId { get; set; } = "1"; // Default user, will be set from controller
    }
}