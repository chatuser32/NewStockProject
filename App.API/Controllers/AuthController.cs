using App.API.Auth;
using App.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace App.API.Controllers
{
    [ApiController]
    [Route("auth")] 
    public class AuthController(IAuthService authService, ITokenService tokenService, IOptions<TokenOptions> tokenOptions) : ControllerBase
    {
        public record LoginRequest(string Username, string Password);
        public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc);
        public record RegisterRequest(string Username, string FullName, string Email, string Password);

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await authService.AuthenticateAsync(request.Username, request.Password);
            if (!result.Success)
            {
                return Unauthorized(new { error = result.Error });
            }

            var opts = tokenOptions.Value;
            var token = tokenService.CreateToken(result.User!.Id, result.User.Username, result.User.FullName, result.Roles, opts, out var exp);
            return Ok(new LoginResponse(token, exp));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await authService.RegisterAsync(request.Username, request.FullName, request.Email, request.Password);
            if (!result.Success)
            {
                return BadRequest(new { error = result.Error });
            }
            return Ok(new { message = "Kullanıcı oluşturuldu.", userId = result.UserId });
        }
    }
}

