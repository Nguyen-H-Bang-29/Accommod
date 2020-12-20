using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Dtos;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet("ward")]
        [ProducesResponseType(typeof(WardDto), 200)]
        public async Task<IActionResult> GetWard([FromQuery] string wardCode)
        {
            return Ok(_locationService.GetWard(wardCode));
        }

        [HttpGet("wards")]
        [ProducesResponseType(typeof(List<WardDto>), 200)]
        public async Task<IActionResult> GetWardsByDistrict([FromQuery] string districtCode)
        {
            return Ok(await _locationService.GetWardsByDistrict(districtCode));
        }

        [HttpGet("districts")]
        [ProducesResponseType(typeof(List<DistrictDto>), 200)]
        public async Task<IActionResult> GetDistrictsByProvince([FromQuery] string provinceCode)
        {
            return Ok(await _locationService.GetDistrictsByProvince(provinceCode));
        }

        [HttpGet("provinces")]
        [ProducesResponseType(typeof(List<ProvinceDto>), 200)]
        public async Task<IActionResult> GetProvinces()
        {
            return Ok(await _locationService.GetProvinces());
        }
    }
}
