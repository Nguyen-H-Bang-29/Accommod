using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Entities;

namespace WebApi.Dtos
{
    public class GetReviewDto
    {
        public string PostId;
        public string UserId;
        public bool Viewed;
        public bool Reported;
        public int Rating;
        public List<Comment> Comments;
    }
}
