using System.Text.Json.Serialization;

namespace WebApi.Entities
{
    public class User : EntityBase
    {
        public string Username { get; set; }
        public string Role { get; set; }
        [JsonIgnore]
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public UserStatus Status { get; set; } 
    }
    public enum UserStatus
    {
        Pending, 
        Approved,
        Rejected
    }
}