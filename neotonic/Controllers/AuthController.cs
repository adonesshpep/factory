using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using neotonic.DTOs;
using neotonic.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace neotonic.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditLogService _auditLogService;
        private readonly IConfiguration _config;
        public AuthController(UserManager<AppUser> userManager, IConfiguration config, RoleManager<IdentityRole> roleManager,IAuditLogService auditLogService)
        {
            _userManager = userManager;
            _config = config;
            _roleManager = roleManager;
            _auditLogService = auditLogService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized();

            if (!user.IsActive)
            {
                await _auditLogService.LogAsync(
    user.Id,
    user.Email,
    await _userManager.GetRolesAsync(user),
    "LoginRejected",
    $"Deactivated user {user.UserName} attempted to log in",
    HttpContext.Connection.RemoteIpAddress?.ToString());
                return Unauthorized(new { message = "This account has been deactivated" });
            }

            var token = await GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);
            var loginTime = DateTime.UtcNow;

            await _auditLogService.LogAsync(
                user.Id,
                user.Email,
                roles,
                "Login",
                $"User {user.UserName} logged in",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    userName = user.UserName,
                    fullName = user.FullName,
                    email = user.Email,
                    roles = roles,
                    loginTime = loginTime
                }
            });
        }
        [Authorize(Policy = "ActiveUser")]
        [Authorize(Roles = "Admin")]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(AssignRoleDto dto)
        {
            // Confirm the role actually exists before trying to assign it
            var roleExists = await _roleManager.RoleExistsAsync(dto.Role);
            if (!roleExists)
                return BadRequest(new { message = $"Role '{dto.Role}' does not exist" });

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user is null)
                return NotFound(new { message = "User not found" });

            // Optional: remove existing roles first if you want a single-role-per-user model
            // var currentRoles = await _userManager.GetRolesAsync(user);
            // await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var result = await _userManager.AddToRoleAsync(user, dto.Role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);
            await _auditLogService.LogAsync(
    User, // the current ClaimsPrincipal, available in every controller via the `User` property
    "AddRole",
    $"Add role '{dto.Role}' to user {user.Email}",
    HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"Role '{dto.Role}' assigned to user {user.Email}" });
        }
        [Authorize(Policy = "ActiveUser")]
        [Authorize(Roles = "Admin")]
        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRole(RemoveRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user is null)
            {
                return NotFound(new { message = "User Not Found" });
            }
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Any())
            {
                return BadRequest(new { message="The Current User Does Not Have Any Roles"});
            }
            if (!currentRoles.Contains(dto.Role))
            {
                return NotFound(new { message = "The Role You Asked To Be Removed Is Not Found In The User Roles" });
            }
            if (dto.Role.Contains("Admin") || dto.Role.Contains("admin"))
            {
                return Unauthorized(new { message = "you cannot remove the admin role from another admin" });
            }
            var result=await _userManager.RemoveFromRoleAsync(user, dto.Role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            await _auditLogService.LogAsync(
                User, // the current ClaimsPrincipal, available in every controller via the `User` property
                "RemoveRole",
                $"Removed role '{dto.Role}' from user {user.Email}",
                HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(new { message = "Roles Was Removed" });
        }
        private async Task<string> GenerateJwtToken(AppUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
        };
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
