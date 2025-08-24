using App.Repositories.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.API.Controllers
{
    [ApiController]
    [Route("users")]
    [Authorize(Roles = "Admin")]
    public class UsersController(IUserRepository userRepository) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await userRepository.GetAll().Select(u => new { u.Id, u.Username, u.FullName, u.Email, u.IsActive }).ToListAsync();
            return Ok(users);
        }
    }
}

