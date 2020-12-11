using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Entities
{
    public class Post : EntityBase
    {
        public string Caption { get; set; }
        public string HostId { get; set; }
        public PostStatus Status { get; set; }
    }
    public enum PostStatus
    {
        Pending,
        Rejected,
        Available,
        Occupied
    }
}
