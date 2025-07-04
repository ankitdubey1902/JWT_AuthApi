using JWT_AuthApi.Models.Auth;
using JWT_AuthApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JWT_AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IJwtService _jwtService;

        public AuthController(AuthService authService, IJwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthenticateRequest model)
        {
            try
            {
                var user = new User
                {
                    Username = model.Username,
                    Email = $"{model.Username}@example.com" // In real app, this would come from request
                };

                await _authService.Register(user, model.Password, GetIpAddress());
                var response = await _authService.GenerateTokenResponse(user, GetIpAddress());

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthenticateRequest model)
        {
            var user = await _authService.Authenticate(model.Username, model.Password, GetIpAddress());

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var response = await _authService.GenerateTokenResponse(user, GetIpAddress());

            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            try
            {
                var response = await _authService.RefreshToken(model.RefreshToken, GetIpAddress());
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest model)
        {
            try
            {
                await _authService.RevokeToken(model.RefreshToken, GetIpAddress());
                return Ok(new { message = "Token revoked" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var username = User.FindFirst(ClaimTypes.Name).Value;
            var email = User.FindFirst(ClaimTypes.Email).Value;

            return Ok(new
            {
                Id = userId,
                Username = username,
                Email = email
            });
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }
    }
}