using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Dtos;

namespace WebApi.Entities
{
    public class Notification : EntityBase
    {
        public GetPostDto Post { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public bool Success { get; set; }
    }
}
