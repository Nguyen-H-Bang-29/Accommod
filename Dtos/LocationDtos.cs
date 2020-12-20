using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Dtos
{
    public class ProvinceDto
    {
        public string Province;
        public string  ProvinceCode;
    }
    public class DistrictDto
    {
        public string District;
        public string DistrictCode;
        public ProvinceDto Province;
    }
    public class WardDto
    {
        public string Ward;
        public string WardCode;
        public DistrictDto District;
    }
}
