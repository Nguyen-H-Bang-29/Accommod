using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Entities
{
    public enum PostStatus
    {
        Pending,
        Rejected,
        Available,
        Occupied
    }
    public class Post : EntityBase
    {
        public string Caption { get; set; }
        public string HostId { get; set; }
        public PostStatus Status { get; set; }
        public string WardCode { get; set; }
        public string Address { get; set; }
        public long Rent { get; set; }
        public List<string> Photos { get; set; }
    }
}
