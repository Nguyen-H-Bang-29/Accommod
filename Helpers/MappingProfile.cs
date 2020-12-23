using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Dtos;
using WebApi.Entities;
using WebApi.Services;

namespace WebApi.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Post, GetPostDto>();
            CreateMap<Review, GetReviewDto>();
            CreateMap<User, HostDto>();
        }
    }
}
