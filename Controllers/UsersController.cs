using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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

        [HttpPost("authenticate")]
        [ProducesResponseType(typeof(AuthenticateResponse), 200)]
        [ProducesResponseType(401)]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            try
            {
                var response = _userService.Authenticate(model);
                return Ok(response);
            }
            catch (Exception e)
            {
                return StatusCode(401, e.Message);
            }
        }

        [HttpPost("signin")]
        [ProducesResponseType(typeof(AuthenticateResponse), 200)]
        [ProducesResponseType(401)]
        public IActionResult SignIn(SignInRequest model)
        {
            try
            {
                var response = _userService.SignIn(model);
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
    }
}
