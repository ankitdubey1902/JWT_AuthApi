using JWT_AuthApi.Models;
using JWT_AuthApi.Models.Auth;
using JWT_AuthApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JWT_AuthApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SampleController : ControllerBase
    {
        private readonly AuthDemoDbContext _dbContext;

        public SampleController(AuthDemoDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet("protected")]
        public async Task<IActionResult> GetProtectedData()
        {
            // Access user info from claims
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var username = User.FindFirst(ClaimTypes.Name).Value;
            var email = User.FindFirst(ClaimTypes.Email).Value;

            // Fetch additional user data from database
            var user = await _dbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found");


            var response = new SampleResponse
            {
                Message = $"Hello {username}! This is protected data. Your email id is - {email}",
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                CreatedDate = user.CreatedAt
            };

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("public")]
        public IActionResult GetPublicData()
        {
            return Ok(new
            {
                Message = "This is public data accessible to everyone.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}