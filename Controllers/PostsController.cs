using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Dtos;
using WebApi.Entities;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _postService.GetAll();
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            try
            {
                var result = await _postService.GetById(id);
                return Ok(result);
            }
            catch (KeyNotFoundException e)
            {
                return StatusCode(400, e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate(CreateOrUpdatePostDto input)
        {
            try
            {
                var result = await _postService.CreateOrUpdate(input);
                return Ok(result);
            }
            catch (KeyNotFoundException e)
            {
                return StatusCode(400, e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [Authorize]
        [HttpDelete("{id}")]        
        public async Task<IActionResult> Delete([FromRoute]string id)
        {
            try
            {                
                return Ok(await _postService.Delete(id));
            }
            catch (KeyNotFoundException e)
            {
                return StatusCode(400, e.Message);
            }            
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [Authorize]
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve([FromRoute] string id)
        {
            try
            {
                return Ok(await _postService.Approve(id));
            }
            catch (KeyNotFoundException e)
            {
                return StatusCode(400, e.Message);
            }
            catch (InvalidOperationException e)
            {
                return StatusCode(400, e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [Authorize]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject([FromRoute] string id)
        {
            try
            {
                return Ok(await _postService.Reject(id));
            }
            catch (KeyNotFoundException e)
            {
                return StatusCode(400, e.Message);
            }
            catch (InvalidOperationException e)
            {
                return StatusCode(400, e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    
    }
}
