using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        AuthenticateResponse SignIn(SignInRequest model);
        IEnumerable<User> GetAll();
        User GetById(string id);
    }

    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private readonly IMongoCollection<User> _users; 

        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings, IAccommodDatabaseSettings dbSettings)
        {
            _appSettings = appSettings.Value;

            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);

            _users = Util.GetCollection<User>(database, dbSettings.UsersCollectionName);
        } 

        public AuthenticateResponse SignIn(SignInRequest model)
        {
            using SHA256 mySHA256 = SHA256.Create();
            var user = _users.Find(x => x.Username == model.Username).SingleOrDefault();
            if (user != null) throw new Exception(message: "Tên đăng nhập đã tồn tại");
            user = new User()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username,
                Password = Util.GetHashString(mySHA256.ComputeHash(Encoding.ASCII.GetBytes(model.Password))),
                Role = model.Role,
                CreatedTime = DateTime.UtcNow
            };
            _users.InsertOne(user);
            var token = GenerateJwtToken(user);
            return new AuthenticateResponse(user, token);
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            using SHA256 mySHA256 = SHA256.Create();
            var user = _users.Find(x => x.Username == model.Username &&
                x.Password == Util.GetHashString(mySHA256.ComputeHash((Encoding.ASCII.GetBytes(model.Password))))).SingleOrDefault();

            // return null if user not found
            if (user == null) throw new Exception(message: "Tên đăng nhập hoặc mật khẩu không chính xác");

            // authentication successful so generate jwt token
            var token = GenerateJwtToken(user);

            return new AuthenticateResponse(user, token);

        }

        public IEnumerable<User> GetAll()
        {
            return _users.Find(_ => true).ToEnumerable();
        }

        public User GetById(string id)
        {
            return _users.AsQueryable().FirstOrDefault(x => x.Id == id);
        }

        // helper methods

        private string GenerateJwtToken(User user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("id", user.Id.ToString()), 
                    new Claim("role", user.Role) }
                ),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}