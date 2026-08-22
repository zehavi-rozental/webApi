namespace MyMiddleware.Models
{
    public class UserIceCreamRankingDto
    {
        public string Name { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int UserRank { get; set; }
    }
}