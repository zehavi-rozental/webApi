using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MyMiddleware.Models
{
    public class IceCream
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Milki { get; set; }
        [ReadOnly(true)]
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;
    }
}