using App.Repositories.Roles;
using App.Repositories.UserRoles;
using App.Repositories.Users;
using App.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.API.Controllers
{
    [ApiController]
    [Route("roles")]
    [Authorize(Roles = "Admin")]
    public class RolesController(IUserRepository userRepository, IRoleRepository roleRepository, IGenericRepository<UserRole> userRoleRepository) : ControllerBase
    {
        public record AssignRoleRequest(int UserId, string RoleName);

        [HttpPost("assign")]
        public async Task<IActionResult> Assign([FromBody] AssignRoleRequest request)
        {
            var user = await userRepository.GetByIdAsync(request.UserId);
            if (user is null) return NotFound(new { error = "Kullanıcı bulunamadı." });

            var role = await roleRepository.Where(r => r.Name == request.RoleName).FirstOrDefaultAsync();
            if (role is null) return NotFound(new { error = "Rol bulunamadı." });

            var exists = await userRoleRepository.Where(ur => ur.UserId == request.UserId && ur.RoleId == role.Id).AnyAsync();
            if (!exists)
            {
                await userRoleRepository.AddAsync(new UserRole { UserId = request.UserId, RoleId = role.Id });
            }
            return Ok(new { message = "Rol atandı." });
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await roleRepository.GetAll().Select(r => r.Name).ToListAsync();
            return Ok(roles);
        }
    }
}

