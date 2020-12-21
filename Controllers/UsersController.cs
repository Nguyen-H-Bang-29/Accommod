using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Entities;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthenticateResponse), 200)]
        [ProducesResponseType(401)]
        public IActionResult LogIn(AuthenticateRequest model)
        {
            try
            {
                var response = _userService.LogIn(model);
                return Ok(response);
            }
            catch (Exception e)
            {
                return StatusCode(401, e.Message);
            }
        }

        [HttpPost("signup")]
        [ProducesResponseType(typeof(AuthenticateResponse), 200)]
        [ProducesResponseType(401)]
        public IActionResult SignUp(SignUpRequest model)
        {
            try
            {
                var response = _userService.SignUp(model);
                return Ok(response);
            }
            catch(Exception e)
            {
                return StatusCode(401, e.Message);
            }
        }

        [Authorize(Role.Admin)]
        [HttpGet]
        [ProducesResponseType(typeof(List<User>), 200)]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [Authorize(Role.Admin)]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(List<User>), 200)]
        public IActionResult Get([FromRoute] string id)
        {
            var user = _userService.GetById(id);
            return Ok(user);
        }

        [Authorize(Role.Admin)]
        [HttpPost("{id}/approve")]
        [ProducesResponseType(typeof(List<User>), 200)]
        public async Task<IActionResult> Approve([FromRoute] string id)
        {
            return Ok(await _userService.Approve(id));
        }

        [Authorize(Role.Admin)]
        [HttpPost("{id}/reject")]
        [ProducesResponseType(typeof(List<User>), 200)]
        public async Task<IActionResult> Reject([FromRoute] string id)
        {
            return Ok(await _userService.Reject(id));
        }
    }
}
