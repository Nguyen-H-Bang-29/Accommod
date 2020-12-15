using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Entities
{
    public class Comment
    {
        public string Content { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
