using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Entities;

namespace WebApi.Dtos
{
    public class CreateOrUpdatePostDto
    {        
        [AllowNull]
        public string Id;
        public string Caption;
        public string HostId;
    }
    public class GetPostDto
    {
        public string Id;
        public string Caption;
        public string HostId;
        public PostStatus Status;
    }    
}
