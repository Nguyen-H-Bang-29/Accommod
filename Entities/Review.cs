using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Entities
{
    public class Review : EntityBase
    {
        public string PostId { get; set; }
        public string UserId { get; set; }
        public bool Viewed { get; set; }
        public bool Reported { get; set; }
        public int Rating { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
