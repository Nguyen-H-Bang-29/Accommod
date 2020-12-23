using AutoMapper;
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
using System.Threading.Tasks;
using WebApi.Dtos;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Services
{
    public interface IUserService
    {
        AuthenticateResponse LogIn(AuthenticateRequest model);
        AuthenticateResponse SignUp(SignUpRequest model);
        List<User> GetAll();
        User GetById(string id);
        Task<User> Approve(string id);
        Task<User> Reject(string id);
        public Task<SearchResultUserDto> Search(SearchUserDto searchParam);

    }

    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private readonly IMongoCollection<User> _users; 

        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;

        public UserService(IOptions<AppSettings> appSettings, IAccommodDatabaseSettings dbSettings, IMapper mapper)
        {
            _appSettings = appSettings.Value;

            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);

            _users = Util.GetCollection<User>(database, dbSettings.UsersCollectionName);
            _mapper = mapper;
        } 

        public AuthenticateResponse SignUp(SignUpRequest model)
        {
            using SHA256 mySHA256 = SHA256.Create();
            var user = _users.Find(x => x.Username == model.Username).SingleOrDefault();
            if (user != null) throw new Exception(message: "Tên đăng nhập đã tồn tại");
            user = new User()
            {
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Username = model.Username,
                Password = Util.GetHashString(mySHA256.ComputeHash(Encoding.ASCII.GetBytes(model.Password))),
                Role = model.Role,
                CreatedTime = DateTime.UtcNow,
                Status = model.Role == Role.Renter ? UserStatus.Approved : UserStatus.Pending
            };
            _users.InsertOne(user);
            var token = GenerateJwtToken(user);
            return new AuthenticateResponse(user, token);
        }

        public AuthenticateResponse LogIn(AuthenticateRequest model)
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

        public async Task<User> Approve(string id)
        {
            return await UpdateStatus(id, UserStatus.Approved);
        }

        public async Task<User> Reject(string id)
        {
            return await UpdateStatus(id, UserStatus.Rejected);
        }

        public List<User> GetAll()
        {
            return _users.Find(_ => true).ToList();
        }

        public User GetById(string id)
        {
            return _users.AsQueryable().FirstOrDefault(x => x.Id == id);
        }

        // helper methods
        private async Task<User> UpdateStatus(string id, UserStatus status)
        {
            var user = _users.AsQueryable().FirstOrDefault(x => x.Id == id);
            if (user == null)
                throw new KeyNotFoundException("Không tồn tại bản ghi với Id được cung cấp");
            if (user.Status != UserStatus.Pending)
                throw new InvalidOperationException("Không thể thay đổi trạng thái bài viết sau khi đã duyệt/từ chối");

            var filter = Builders<User>.Filter.Eq("Id", id);
            var update = Builders<User>.Update.Set("Status", status);
            await _users.UpdateOneAsync(filter, update);

            user = _users.AsQueryable().FirstOrDefault(x => x.Id == id);
            return user;
        }

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
        public async Task<SearchResultUserDto> Search(SearchUserDto searchParam)
        {
            UserStatus[] statuses = searchParam.ShowRejected ? new UserStatus[] { UserStatus.Pending, UserStatus.Rejected, UserStatus.Approved }
                    : new UserStatus[] { UserStatus.Pending, UserStatus.Approved };                           

            List<User> users = _users.AsQueryable()
                .Where(p => statuses.Contains(p.Status)).ToList()
                .Where(p =>
                    p.Username.Contains(searchParam.Keyword))
                .ToList();
            IEnumerable<User> raw = users;
            var count = users.Count();
           
            var result = raw.Skip(searchParam.Skip)
                        .Take(searchParam.Take).Select(p => _mapper.Map<HostDto>(p)).ToList();
            return new SearchResultUserDto()
            {
                Result = result,
                Count = count,
                StartIndex = searchParam.Skip,
            };
        }

    }
}