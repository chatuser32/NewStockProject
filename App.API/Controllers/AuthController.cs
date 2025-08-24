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
        public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, string RefreshToken, DateTime RefreshExpiresAtUtc);
        public record RegisterRequest(string Username, string FullName, string Email, string Password);
        public record RefreshRequest(string RefreshToken);
        public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
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
            var rt = await authService.IssueRefreshTokenAsync(result.User.Id);
            return Ok(new LoginResponse(token, exp, rt.RefreshToken!, rt.ExpiresAtUtc!.Value));
        }

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
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

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var result = await authService.ValidateRefreshTokenAsync(request.RefreshToken);
            if (!result.Success)
                return Unauthorized(new { error = result.Error });

            var opts = tokenOptions.Value;
            var token = tokenService.CreateToken(result.User!.Id, result.User.Username, result.User.FullName, result.Roles, opts, out var exp);
            var rt = await authService.IssueRefreshTokenAsync(result.User.Id);
            return Ok(new LoginResponse(token, exp, rt.RefreshToken!, rt.ExpiresAtUtc!.Value));
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            // userId, token claimlerinden alınır
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            if (!result.Success)
                return BadRequest(new { error = result.Error });
            return Ok(new { message = "Parola güncellendi." });
        }
    }
}

