using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Entities
{
    [BsonIgnoreExtraElements]
    public class Location : EntityBase
    {
        public string ProvinceCode { get; set; }
        public string DistrictCode { get; set; }
        public string WardCode { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
    }
}
