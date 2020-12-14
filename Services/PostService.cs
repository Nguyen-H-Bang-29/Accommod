using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Dtos;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface IPostService
    {
        public Task<List<GetPostDto>> GetAll();
        public Task<GetPostDto> CreateOrUpdate(CreateOrUpdatePostDto input, string hostId);
        public Task<GetPostDto> GetById(string id);
        public Task<GetPostDto> Delete(string id);
        public Task<GetPostDto> Approve(string id);
        public Task<GetPostDto> Reject(string id);
    }
    public class PostService : IPostService
    {
        private readonly IMapper _mapper;
        private readonly IMongoCollection<Post> _posts;
        private readonly IMongoCollection<User> _users;
        public PostService(IAccommodDatabaseSettings settings, IMapper mapper)
        {
            _mapper = mapper;

            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _posts = Util.GetCollection<Post>(database, settings.PostsCollectionName);
            _users = Util.GetCollection<User>(database, settings.UsersCollectionName);
        }

        public async Task<List<GetPostDto>> GetAll()
        {
            var posts = await (await _posts.FindAsync(_ => true)).ToListAsync();
            var result = posts.Select(p => _mapper.Map<GetPostDto>(p)).ToList();
            return result;
        }

        public async Task<GetPostDto> GetById(string id)
        {
            var post = _posts.AsQueryable().FirstOrDefault(x => x.Id == id);
            if (post == null) throw new KeyNotFoundException("Không tồn tại bản ghi với Id được cung cấp");
            return _mapper.Map<GetPostDto>(post);
        }

        public async Task<GetPostDto> CreateOrUpdate(CreateOrUpdatePostDto input, string hostId)
        {            
            Post post = new Post()
            {
                Id = input.Id,
                Caption = input.Caption,
                HostId = hostId,
                Status = _users.AsQueryable().FirstOrDefault(u => u.Id == hostId).Role == Role.Admin ? PostStatus.Available : PostStatus.Pending
            };
            if (input.Id == null || input.Id == "")
            {
                post.CreatedTime = DateTime.UtcNow;
                await _posts.InsertOneAsync(post);
            }
            else
            {
                var filter = Builders<Post>.Filter.Eq("Id", post.Id);
                post = await _posts.FindOneAndReplaceAsync(filter, post);
                if (post == null) throw new KeyNotFoundException("Không tồn tại bản ghi với Id được cung cấp");
            }            
            return _mapper.Map<GetPostDto>(post);
        }

        public async Task<GetPostDto> Delete(string id)
        {
            var filter = Builders<Post>.Filter.Eq("Id", id);
            var post = await _posts.FindOneAndDeleteAsync(filter);
            if (post == null) throw new KeyNotFoundException("Không tồn tại bản ghi với Id được cung cấp");
            return _mapper.Map<GetPostDto>(post);
        }

        public async Task<GetPostDto> Approve(string id)
        {
            return await UpdateStatus(id, PostStatus.Available);
        }

        public async Task<GetPostDto> Reject(string id)
        {
            return await UpdateStatus(id, PostStatus.Rejected);
        }

        //helpers 

        private async Task<GetPostDto> UpdateStatus(string id, PostStatus status)
        {
            var post = _posts.AsQueryable().FirstOrDefault(x => x.Id == id);
            if (post == null)
                throw new KeyNotFoundException("Không tồn tại bản ghi với Id được cung cấp");
            if (post.Status != PostStatus.Pending)
                throw new InvalidOperationException("Không thể thay đổi trạng thái bài viết sau khi đã duyệt/từ chối");

            var filter = Builders<Post>.Filter.Eq("Id", id);
            var update = Builders<Post>.Update.Set("Status", status);
            await _posts.FindOneAndUpdateAsync(filter, update);

            post = _posts.AsQueryable().FirstOrDefault(x => x.Id == id);
            return _mapper.Map<GetPostDto>(post);
        }
    }
}
