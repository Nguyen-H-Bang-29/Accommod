﻿using AutoMapper;
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
    public interface IReviewService
    {
        // CRUD
        public Task<GetReviewDto> View(string postId, string userId);
        public Task<GetReviewDto> Rate(string postId, string userId, int star);
        public Task<GetReviewDto> Report(string postId, string userId);
        public Task<GetReviewDto> Comment(string postId, string userId, string content);
        public Task<GetReviewDto> AddFavorite(string postId, string userId);
        public Task<GetReviewDto> RemoveFavorite(string postId, string userId);

        public double GetRating(string postId);
        public int GetViews(string postId);
        public int GetReports(string postId);
        public Task<List<Comment>> GetComments(string postId);
        public Task<List<Post>> GetFavorites(string userId);
    }
    public class ReviewService : IReviewService
    {
        private readonly IMapper _mapper;
        private readonly IMongoCollection<Post> _posts;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Review> _reviews;

        public ReviewService(IAccommodDatabaseSettings settings, IMapper mapper)
        {
            _mapper = mapper;

            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _posts = Util.GetCollection<Post>(database, settings.PostsCollectionName);
            _users = Util.GetCollection<User>(database, settings.UsersCollectionName);
            _reviews = Util.GetCollection<Review>(database, settings.ReviewsCollectionName);
        }

        #region CRUD

        public double GetRating(string postId)
        {
            if (_posts.AsQueryable().FirstOrDefault(p => p.Id == postId) == null) throw new KeyNotFoundException("Không tìm thấy bài đăng");
            var reviews = _reviews.AsQueryable()
                .Where(r => r.PostId == postId && Enumerable.Range(1, 5).Contains(r.Rating));
            if (reviews.Count() == 0) return 0;
            return reviews.Average(r => r.Rating);
        }

        public int GetViews(string postId)
        {
            if (_posts.AsQueryable().FirstOrDefault(p => p.Id == postId) == null) throw new KeyNotFoundException("Không tìm thấy bài đăng");
            var reviews = _reviews.AsQueryable()
                .Where(r => r.PostId == postId && r.Viewed);
            if (reviews.Count() > 0) return reviews
                 .Count();
            return 0;
        }

        public int GetReports(string postId)
        {
            if (_posts.AsQueryable().FirstOrDefault(p => p.Id == postId) == null) throw new KeyNotFoundException("Không tìm thấy bài đăng");
            return _reviews.AsQueryable()
                .Where(r => r.PostId == postId && r.Reported)
                .Count();
        }

        public async Task<List<Comment>> GetComments(string postId)
        {
            if (_posts.AsQueryable().FirstOrDefault(p => p.Id == postId) == null) throw new KeyNotFoundException("Không tìm thấy bài đăng");
            return _reviews.AsQueryable()
                .Where(r => r.PostId == postId)
                .SelectMany(r => r.Comments)
                .OrderBy(c => c.CreatedTime).ToList();
        }

        public async Task<GetReviewDto> View(string postId, string userId)
        {
            if (_posts.AsQueryable().FirstOrDefault(p => p.Id == postId) == null) throw new KeyNotFoundException("Không tìm thấy bài đăng");
            var review = await Get(postId, userId);
            review.Viewed = true;
            return _mapper.Map<GetReviewDto>(await Update(review));
        }

        public async Task<GetReviewDto> Rate(string postId, string userId, int star)
        {
            if (_posts.AsQueryable().FirstOrDefault(p => p.Id == postId) == null) throw new KeyNotFoundException("Không tìm thấy bài đăng");
            var review = await Get(postId, userId);
            review.Rating = star;
            return _mapper.Map<GetReviewDto>(await Update(review));
        }

        public async Task<GetReviewDto> Report(string postId, string userId)
        {
            if (_posts.AsQueryable().FirstOrDefault(p => p.Id == postId) == null) throw new KeyNotFoundException("Không tìm thấy bài đăng");
            var review = await Get(postId, userId);
            review.Reported = true;
            return _mapper.Map<GetReviewDto>(await Update(review));
        }

        public async Task<GetReviewDto> Comment(string postId, string userId, string content)
        {
            if (_posts.AsQueryable().FirstOrDefault(p => p.Id == postId) == null) throw new KeyNotFoundException("Không tìm thấy bài đăng");
            var review = await Get(postId, userId);
            review.Comments.Add(new Entities.Comment()
            {
                Content = content,
                CreatedTime = DateTime.UtcNow
            });
            return _mapper.Map<GetReviewDto>(await Update(review));
        }

        public async Task<GetReviewDto> AddFavorite(string postId, string userId)
        {
            if (_posts.AsQueryable().FirstOrDefault(p => p.Id == postId) == null) throw new KeyNotFoundException("Không tìm thấy bài đăng");
            var review = await Get(postId, userId);
            review.IsFavorite = true;
            return _mapper.Map<GetReviewDto>(await Update(review));
        }
        public async Task<GetReviewDto> RemoveFavorite(string postId, string userId)
        {
            if (_posts.AsQueryable().FirstOrDefault(p => p.Id == postId) == null) throw new KeyNotFoundException("Không tìm thấy bài đăng");
            var review = await Get(postId, userId);
            review.IsFavorite = false;
            return _mapper.Map<GetReviewDto>(await Update(review));
        }
        public async Task<List<Post>> GetFavorites(string userId)
        {
            var postIds = _reviews.AsQueryable().Where(r => r.UserId == userId).Where(r => r.IsFavorite).Select(r => r.PostId).ToList();
            var posts = _posts.AsQueryable().Where(p => postIds.Contains(p.Id));
            return posts.ToList();
        }

        #endregion

        #region helpers
        private async Task<Review> Get(string postId, string userId)
        {
            if (_posts.AsQueryable().FirstOrDefault(p => p.Id == postId) == null) throw new KeyNotFoundException("Không tìm thấy bài đăng");
            var review = _reviews.AsQueryable().FirstOrDefault(r => r.PostId == postId && r.UserId == userId);
            if (review == null)
            {
                review = new Review()
                {
                    CreatedTime = DateTime.UtcNow,
                    PostId = postId,
                    UserId = userId,
                    Comments = new List<Comment>(),
                    Rating = 0,
                    Reported = false,
                    Viewed = false,
                    IsFavorite = false
                };
                await _reviews.InsertOneAsync(review);
            }
            return review;
        }

        private async Task<Review> Update(Review input)
        {
            if (_posts.AsQueryable().FirstOrDefault(p => p.Id == input.PostId) == null) throw new KeyNotFoundException("Không tìm thấy bài đăng");
            var review = await Get(input.PostId, input.UserId);
            review.Reported = input.Reported;
            review.Rating = input.Rating;
            review.Viewed = input.Viewed;
            review.Comments = input.Comments;
            review.IsFavorite = input.IsFavorite;
            review = await _reviews.FindOneAndReplaceAsync(
                r => r.PostId == input.PostId && r.UserId == input.UserId, review);
            return await Get(review.PostId, review.UserId);
        }


        #endregion

    }
}
