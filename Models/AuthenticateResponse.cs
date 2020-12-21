using MongoDB.Bson;
using WebApi.Entities;

namespace WebApi.Models
{
    public class AuthenticateResponse
    {
        public string Username { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Token { get; set; }


        public AuthenticateResponse(User user, string token)
        {
            Username = user.Username;
            Role = user.Role;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
            Token = token;
        }
    }
}