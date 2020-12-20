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
        private readonly IReviewService _reviewService;

        public PostsController(IPostService postService, IReviewService reviewService)
        {
            _postService = postService;
            _reviewService = reviewService;
        }

        #region Host

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(List<GetPostDto>), 200)]
        [ProducesResponseType(500)]
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
        [ProducesResponseType(typeof(GetPostDto), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
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

        [Authorize(Role.Admin, Role.Host)]
        [HttpPost]
        [ProducesResponseType(typeof(GetPostDto), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateOrUpdate(CreateOrUpdatePostDto input)
        {
            var host = (User)HttpContext.Items["User"];

            try
            {
                var result = await _postService.CreateOrUpdate(input, host.Id);
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

        [Authorize(Role.Admin, Role.Host)]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(GetPostDto), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
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

        #endregion

        #region Admin

        [Authorize(Role.Admin)]
        [HttpPut("{id}/approve")]
        [ProducesResponseType(typeof(GetPostDto), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
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

        [Authorize(Role.Admin)]
        [HttpPut("{id}/reject")]
        [ProducesResponseType(typeof(GetPostDto), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
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

        #endregion

        #region Renter

        [Authorize(Role.Renter)]
        [HttpPost("{id}/view")]
        [ProducesResponseType(typeof(GetReviewDto), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> View([FromRoute] string id)
        {
            var host = (User)HttpContext.Items["User"];
            try
            {
                return Ok(await _reviewService.View(id, host.Id));
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

        [Authorize(Role.Renter)]
        [HttpPost("{id}/rating")]
        [ProducesResponseType(typeof(GetReviewDto), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Rate([FromRoute] string id, [FromQuery] int rating )
        {
            var host = (User)HttpContext.Items["User"];

            try
            {
                return Ok(await _reviewService.Rate(id, host.Id, rating));
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

        [Authorize(Role.Renter)]
        [HttpPost("{id}/report")]
        [ProducesResponseType(typeof(GetReviewDto), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Report([FromRoute] string id)
        {
            var host = (User)HttpContext.Items["User"];

            try
            {
                return Ok(await _reviewService.Report(id, host.Id));
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

        [Authorize(Role.Renter)]
        [HttpPost("{id}/comment")]
        [ProducesResponseType(typeof(GetReviewDto), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Comment([FromRoute] string id, [FromBody] dynamic body)
        {
            var host = (User)HttpContext.Items["User"];

            try
            {
                return Ok(await _reviewService.Comment(id, host.Id, body.content.ToString()));
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

        [Authorize(Role.Renter)]
        [HttpGet("{id}/view")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetViews([FromRoute] string id)
        {
            try
            {
                return Ok(_reviewService.GetViews(id));
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

        [Authorize(Role.Renter)]
        [HttpGet("{id}/rating")]
        [ProducesResponseType(typeof(double), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetRating([FromRoute] string id)
        {
            try
            {
                return Ok(_reviewService.GetRating(id));
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

        [Authorize(Role.Renter)]
        [HttpGet("{id}/report")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetReports([FromRoute] string id)
        {
            try
            {
                return Ok(_reviewService.GetReports(id));
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

        [Authorize(Role.Renter)]
        [HttpGet("{id}/comment")]
        [ProducesResponseType(typeof(List<Comment>), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetComments([FromRoute] string id)
        {
            try
            {
                return Ok(await _reviewService.GetComments(id));
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

        #endregion

        #region Shared

        [HttpPost("search")]
        [Authorize]
        [ProducesResponseType(typeof(SearchResultPostDto), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Search([FromBody]SearchPostDto searchParam)
        {
            string role = HttpContext.Items["Role"].ToString();
            var user = (User)HttpContext.Items["User"];
            try
            {
                return Ok(await _postService.Search(searchParam, role, user.Id));
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        #endregion
    }
}
