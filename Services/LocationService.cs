using AutoMapper;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Dtos;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface ILocationService
    {
        public WardDto GetWard(string wardDto);
        public Task<List<WardDto>> GetWardsByDistrict(string districtCode);
        public Task<List<DistrictDto>> GetDistrictsByProvince(string provinceCode);
        public Task<List<ProvinceDto>> GetProvinces();
        public List<string> GetWardCodesByDistrict(string districtCode);
        public List<string> GetWardCodesByProvince(string provinceCode);


    }

    public class LocationService : ILocationService
    {
        private readonly IMapper _mapper;
        private readonly IMongoCollection<Post> _posts;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Review> _reviews;
        private readonly IMongoCollection<Location> _locations;

        public LocationService(IAccommodDatabaseSettings settings, IMapper mapper)
        {
            _mapper = mapper;

            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _posts = Util.GetCollection<Post>(database, settings.PostsCollectionName);
            _users = Util.GetCollection<User>(database, settings.UsersCollectionName);
            _reviews = Util.GetCollection<Review>(database, settings.ReviewsCollectionName);
            _locations = Util.GetCollection<Location>(database, settings.LocationsCollectionName);
        }

        public async Task<List<DistrictDto>> GetDistrictsByProvince(string provinceCode)
        {
            var location = _locations.AsQueryable().FirstOrDefault(l => l.ProvinceCode == provinceCode);
            var province = new ProvinceDto()
            {
                Province = location.Province,
                ProvinceCode = location.ProvinceCode
            };

            return _locations.AsQueryable().Where(l => l.ProvinceCode == provinceCode).ToList().Select(l => new DistrictDto()
            {
                District = l.District,
                DistrictCode = l.DistrictCode,
                Province = province
            }).GroupBy(d => d.DistrictCode).Select(g => g.FirstOrDefault()).OrderBy(d => d.DistrictCode).ToList();
        }

        public async Task<List<ProvinceDto>> GetProvinces()
        {
            return _locations.AsQueryable().Select(l => new ProvinceDto()
            {
                Province = l.Province,
                ProvinceCode = l.ProvinceCode
            }).Distinct().OrderBy(p => p.ProvinceCode).ToList();
        }

        public WardDto GetWard(string wardCode)
        {
            var location = _locations.AsQueryable().FirstOrDefault(l => l.WardCode == wardCode);
            return new WardDto()
            {
                Ward = location.Ward,
                WardCode = wardCode,
                District = new DistrictDto()
                {
                    District = location.District,
                    DistrictCode = location.DistrictCode,
                    Province = new ProvinceDto()
                    {
                        Province = location.Province,
                        ProvinceCode = location.ProvinceCode
                    }
                }
            };
        }

        public async Task<List<WardDto>> GetWardsByDistrict(string districtCode)
        {
            var location = _locations.AsQueryable().FirstOrDefault(l => l.DistrictCode == districtCode);
            var district = new DistrictDto()
            {
                District = location.District,
                DistrictCode = location.DistrictCode,
                Province = new ProvinceDto()
                {
                    Province = location.Province,
                    ProvinceCode = location.ProvinceCode
                }
            };
            return _locations.AsQueryable().Where(l => l.DistrictCode == districtCode).ToList().Select(l => new WardDto()
            {
                Ward = l.Ward,
                WardCode = l.WardCode,
                District = district
            }).Distinct().OrderBy(w => w.WardCode).ToList();
        }

        public List<string> GetWardCodesByDistrict(string districtCode)
        {
            return _locations.AsQueryable().Where(l => l.DistrictCode == districtCode).ToList().Select(l => l.WardCode).Distinct().OrderBy(w => w).ToList();
        }
        public List<string> GetWardCodesByProvince(string provinceCode)
        {
            return _locations.AsQueryable().Where(l => l.ProvinceCode == provinceCode).ToList().Select(l => l.WardCode).Distinct().OrderBy(w => w).ToList();
        }
    }
}
