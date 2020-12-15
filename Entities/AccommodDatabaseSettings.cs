using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Entities
{
    public class AccommodDatabaseSettings : IAccommodDatabaseSettings
    {
        public string PostsCollectionName { get; set; }
        public string UsersCollectionName { get; set; }
        public string ReviewsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IAccommodDatabaseSettings
    {
        string PostsCollectionName { get; set; }
        string UsersCollectionName { get; set; }
        string ReviewsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
