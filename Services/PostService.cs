using AutoMapper;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApi.Dtos;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface IPostService
    {
        // CRUDAR
        public Task<GetPostDto> CreateOrUpdate(CreateOrUpdatePostDto input, string hostId);
        public Task<GetPostDto> GetById(string id, string role, string userId);
        public Task<GetPostDto> Delete(string id, string role, string userId);
        public Task<GetPostDto> Approve(string id);
        public Task<GetPostDto> Reject(string id);
        public Task<SearchResultPostDto> Search(SearchPostDto searchParam, string role, string userId);
        public Task<List<string>> Upload(string id, List<IFormFile> files);
        public Task<Stream> Download(string id, string fileName);
    }
    public class PostService : IPostService
    {
        private readonly IMapper _mapper;
        private readonly IMongoCollection<Post> _posts;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Review> _reviews;
        private readonly IMongoCollection<Location> _locations;

        private readonly ILocationService _locationService;
        private readonly IReviewService _reviewService;
        private readonly INotificationService _notificationService;
        public PostService(IAccommodDatabaseSettings settings, IMapper mapper, ILocationService locationService, IReviewService reviewService, INotificationService notificationService)
        {
            _mapper = mapper;

            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _posts = Util.GetCollection<Post>(database, settings.PostsCollectionName);
            _users = Util.GetCollection<User>(database, settings.UsersCollectionName);
            _reviews = Util.GetCollection<Review>(database, settings.ReviewsCollectionName);
            _locations = Util.GetCollection<Location>(database, settings.LocationsCollectionName);

            _locationService = locationService;
            _reviewService = reviewService;
            _notificationService = notificationService;
        }

        #region CRUDAR
        private GetPostDto Map(Post post)
        {
            var result = _mapper.Map<GetPostDto>(post);
            result.Ward = _locationService.GetWard(post.WardCode);
            var reviews = _reviews.AsQueryable().Where(r => r.PostId == post.Id);
            result.Rating = GetRating(reviews);
            result.Views = GetViews(reviews);
            var host = _users.Find(h => h.Id == post.HostId).FirstOrDefault();
            result.Host = _mapper.Map<HostDto>(host);
            return result;
        }
        public async Task<GetPostDto> GetById(string id, string role, string userId)
        {
            PostStatus[] statuses;
            switch (role)
            {
                case Role.Host:
                case Role.Admin:
                    statuses = new PostStatus[] { PostStatus.Pending, PostStatus.Rejected, PostStatus.Approved };
                    break;
                case Role.Renter:
                    statuses = new PostStatus[] { PostStatus.Approved };
                    break;
                default:
                    statuses = new PostStatus[] { };
                    break;
            }
            List<string> userIds =
                role == Role.Host ? _users.AsQueryable().Where(u => u.Id == userId).Select(u => u.Id).ToList() :
                new List<string>();
            var post = _posts.AsQueryable()
                .Where(p => userIds.Count() < 1 || userIds.Contains(p.HostId))
                .Where(p => statuses.Contains(p.Status))
                .FirstOrDefault(p => p.Id == id);
            if (post == null) throw new KeyNotFoundException("Không tồn tại bản ghi với Id được cung cấp hoặc bạn không được phép truy cập bản ghi này");
            return Map(post);
        }

        public async Task<GetPostDto> CreateOrUpdate(CreateOrUpdatePostDto input, string hostId)
        {
            Post post = new Post()
            {
                Id = input.Id,
                Caption = input.Caption,
                HostId = hostId,
                Status = _users.AsQueryable().FirstOrDefault(u => u.Id == hostId).Role == Role.Admin ? PostStatus.Approved : PostStatus.Pending,
                WardCode = input.WardCode,
                Address = input.Address,
                Rent = input.Rent,
                Area = input.Area,
                Description = input.Description,
            };
            if (input.Id == null || input.Id == "")
            {
                post.Photos = new List<string>();
                post.CreatedTime = DateTime.UtcNow;
                await _posts.InsertOneAsync(post);
            }
            else
            {
                var filter = Builders<Post>.Filter.Eq("Id", post.Id);
                await _posts.ReplaceOneAsync(filter, post);
                if (post == null) throw new KeyNotFoundException("Không tồn tại bản ghi với Id được cung cấp");
                post = _posts.AsQueryable().FirstOrDefault(x => x.Id == post.Id);
            }
            return Map(post);
        }

        public async Task<GetPostDto> Delete(string id, string role, string userId)
        {
            List<string> userIds =
                role == Role.Host ? _users.AsQueryable().Where(u => u.Id == userId).Select(u => u.Id).ToList() :
                new List<string>();
            var filter = Builders<Post>.Filter.Where(p => p.Id == id);
            var post = await _posts.FindOneAndDeleteAsync(filter);
            if (post == null) throw new KeyNotFoundException("Không tồn tại bản ghi với Id được cung cấp");
            return Map(post);
        }

        public async Task<GetPostDto> Approve(string id)
        {
            return await UpdateStatus(id, PostStatus.Approved);
        }

        public async Task<GetPostDto> Reject(string id)
        {
            return await UpdateStatus(id, PostStatus.Rejected);
        }

        public async Task<SearchResultPostDto> Search(SearchPostDto searchParam, string role, string userId)
        {
            PostStatus[] statuses;
            switch (role)
            {
                case Role.Host:
                case Role.Admin:
                    statuses = searchParam.ShowRejected ? new PostStatus[] { PostStatus.Pending, PostStatus.Rejected, PostStatus.Approved }
                    : new PostStatus[] { PostStatus.Pending, PostStatus.Approved };
                    break;
                case Role.Renter:
                    statuses = new PostStatus[] { PostStatus.Approved };
                    break;
                default:
                    statuses = new PostStatus[] { };
                    break;
            }

            if (searchParam.MinRent == null) searchParam.MinRent = 0;
            if (searchParam.MaxRent == null) searchParam.MaxRent = long.MaxValue;
            List<string> userIds =
                role == Role.Host ? _users.AsQueryable().Where(u => u.Id == userId).Select(u => u.Id).ToList() :
                new List<string>();
            List<string> wardCodes = new List<string>();
            if (searchParam.WardCode != null && searchParam.WardCode != "") wardCodes.Add(searchParam.WardCode);
            else if (searchParam.DistrictCode != null && searchParam.DistrictCode != "")
                wardCodes.AddRange(_locationService.GetWardCodesByDistrict(searchParam.DistrictCode));
            else if (searchParam.ProvinceCode != null && searchParam.ProvinceCode != "")
                wardCodes.AddRange(_locationService.GetWardCodesByProvince(searchParam.ProvinceCode));

            wardCodes = wardCodes.Distinct().Where(c => c != "").ToList();
            var addressKeywords = searchParam.AddressKeyWord.Trim().ToLower().Split(' ').Where(e => e != "").ToArray();
            var captionKeywords = searchParam.CaptionKeyword.Trim().ToLower().Split(' ').Where(e => e != "").ToArray();

            List<Post> posts = _posts.AsQueryable()
                .Where(p => userIds.Count() < 1 || userIds.Contains(p.HostId))
                .Where(p => statuses.Contains(p.Status))
                .Where(p => wardCodes.Count < 1 || wardCodes.Contains(p.WardCode))
                .Where(p => p.Rent <= searchParam.MaxRent && p.Rent >= searchParam.MinRent).ToList()
                .Where(p =>
                    (addressKeywords.Count() < 1 || addressKeywords.Intersect(p.Address.ToLower().Replace(',', ' ').Split(' ')).Count() > 0) &&
                    (captionKeywords.Count() < 1 || captionKeywords.Intersect(p.Caption.ToLower().Replace(',', ' ').Split(' ')).Count() > 0))
                .ToList();
            IEnumerable<Post> raw = posts;
            var count = posts.Count();
            var reviews = _reviews.AsQueryable().ToList();
            switch (searchParam.Sort)
            {
                case SortField.None:
                    raw = posts
                        .Skip(searchParam.Skip)
                        .Take(searchParam.Take);
                    break;
                case SortField.Rent:
                    raw = posts
                        .OrderBy(p => p.Rent);
                    if (searchParam.Desc) raw = raw.Reverse();
                    raw = raw
                   .Skip(searchParam.Skip)
                        .Take(searchParam.Take);
                    break;
                case SortField.Rating:
                    raw =
                        (from p in posts
                         join r in reviews on p.Id equals r.PostId into jo
                         orderby GetRating(jo)
                         select p);
                    if (searchParam.Desc) raw = raw.Reverse();
                    raw = raw
                   .Skip(searchParam.Skip)
                        .Take(searchParam.Take);
                    break;
                case SortField.Views:
                    raw =
                        (from p in posts
                         join r in reviews on p.Id equals r.PostId into jo
                         orderby GetViews(jo)
                         select p);
                    if (searchParam.Desc) raw = raw.Reverse();
                    raw = raw
                   .Skip(searchParam.Skip)
                   .Take(searchParam.Take);
                    break;
            }
            var result = raw.Select(p => Map(p)).ToList();
            return new SearchResultPostDto()
            {
                Result = result,
                Count = count,
                StartIndex = searchParam.Skip,
            };
        }

        public async Task<List<string>> Upload(string id, List<IFormFile> files)
        {
            var post = _posts.Find(p => p.Id == id).FirstOrDefault();
            if (post == null) throw new KeyNotFoundException("Không tồn tại bản ghi với Id được cung cấp");

            var folder = Path.Combine(Util.photosFolder, id + "/");

            List<string> photos = post.Photos;
            foreach (var image in files)
            {
                var title = image.Name + "." + image.FileName.Split('.').LastOrDefault();
                if (!Regex.IsMatch(title, @"^[\w\-. ]+$")) throw new ArgumentException("Tên file được cung cấp không đúng chuẩn");
                Stream s = FileSystemService.GetOrCreateFile(folder, title);
                if (image.Length > 0) await image.CopyToAsync(s);
                s.Close();
                photos.Add(title);
            }

            var filter = Builders<Post>.Filter.Eq("Id", id);
            var update = Builders<Post>.Update.Set(p => p.Photos, photos.Distinct().ToList());
            await _posts.UpdateOneAsync(filter, update);

            return _posts.Find(p => p.Id == id).FirstOrDefault().Photos;
        }

        public async Task<Stream> Download(string postId, string fileName)
        {
            var folder = Path.Combine(Util.photosFolder, postId + "/");
            return FileSystemService.GetFile(folder, fileName);
        }
        #endregion

        #region helpers

        private async Task<GetPostDto> UpdateStatus(string id, PostStatus status)
        {
            var post = _posts.AsQueryable().FirstOrDefault(x => x.Id == id);
            if (post == null)
                throw new KeyNotFoundException("Không tồn tại bản ghi với Id được cung cấp");
            if (post.Status == PostStatus.Approved)
                throw new InvalidOperationException("Không thể thay đổi trạng thái bài viết sau khi đã duyệt");

            var filter = Builders<Post>.Filter.Eq("Id", id);
            var update = Builders<Post>.Update.Set("Status", status);
            await _posts.UpdateOneAsync(filter, update);

            post = _posts.AsQueryable().FirstOrDefault(x => x.Id == id);
            var dto = Map(post);
            if (status == PostStatus.Approved) await _notificationService.Add(new Notification()
            {
                Post = dto,
                UserId = post.HostId,
                Content = "Đã được phê duyệt",
                Success = true,
                CreatedTime = DateTime.UtcNow
            });
            else
            if (status == PostStatus.Rejected) await _notificationService.Add(new Notification()
            {
                Post = dto,
                UserId = post.HostId,
                Content = "Đã bị từ chối",
                CreatedTime = DateTime.UtcNow,
                Success = false
            });
            return dto;
        }

        private double GetRating(IEnumerable<Review> reviews)
        {
            if (reviews.Count() == 0) return 0;
            return reviews.Average(r => r.Rating);
        }

        private int GetViews(IEnumerable<Review> reviews)
        {
            if (reviews.Count() == 0) return 0;
            return reviews.Count(r => r.Viewed);
        }
        #endregion

    }
}
