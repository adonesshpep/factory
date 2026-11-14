using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.Creates;
using neotonic.Services;
using System.IdentityModel.Tokens.Jwt;

namespace neotonic.Controllers
{
    [Route("api/users")]
    [Authorize(Policy = "ActiveUser")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditLogService _auditLogService;
        private readonly AppDbContext _context;
        public UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IAuditLogService auditLogService,AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditLogService = auditLogService;
            _context = context;
        }
        [Authorize(Roles ="Admin")]
        [HttpPost("createuser")]
        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            var rolesToAssign = (dto.Roles is not null && dto.Roles.Any())
                ? dto.Roles
                : new List<string> { "User" };

            foreach (var role in rolesToAssign)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    return BadRequest(new { message = $"Role '{role}' does not exist" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            var user = new AppUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FullName = dto.FullName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                return BadRequest(result.Errors);
            }

            var roleResult = await _userManager.AddToRolesAsync(user, rolesToAssign);

            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Failed to assign roles", errors = roleResult.Errors });
            }

            await transaction.CommitAsync();

            await _auditLogService.LogAsync(
                User,
                "CreateUser",
                $"Created user {user.UserName} with roles: {string.Join(", ", rolesToAssign)}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "User created successfully", userId = user.Id });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("index")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _userManager.Users.OrderBy(u => u.Id);

            var totalCount = await query.CountAsync();
            var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var items = new List<object>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                items.Add(new
                {
                    id = u.Id,
                    userName = u.UserName,
                    fullName = u.FullName,
                    email = u.Email,
                    isActive = u.IsActive,
                    roles = roles
                });
            }

            return Ok(new
            {
                items,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        [Authorize(Roles ="Admin")]
        [HttpDelete("deleteuser/{userid:guid}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user=await _userManager.FindByIdAsync(userId);
            if(user is null)
            {
                return NotFound(new { message = "User Not Found" });
            }
            var targetRoles = await _userManager.GetRolesAsync(user);
            if (targetRoles.Contains("Admin"))
            {
                return BadRequest(new { message = "Admin accounts cannot be deleted by another admin. Use the self-delete endpoint instead." });
            }
            // the real deletion is here
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);
            await _auditLogService.LogAsync(User,"DeleteUser",$"Deleted user {user.UserName} ({user.Email})",HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(new { message = "User deleted successfully" });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("status/{id:guid}")]
        public async Task<IActionResult> ChangeStatus(string id, ChangeStatusDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound(new { message = "User not found" });
            }

            if (!dto.IsActive)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                {
                    var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                    var activeAdminCount = adminUsers.Count(a => a.IsActive);

                    // if this admin is currently active and is the only active one, block deactivation
                    if (user.IsActive && activeAdminCount <= 1)
                    {
                        return BadRequest(new { message = "Cannot deactivate the last remaining active admin account" });
                    }
                }
            }

            user.IsActive = dto.IsActive;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _auditLogService.LogAsync(
                User,
                dto.IsActive ? "ActivateUser" : "DeactivateUser",
                $"{(dto.IsActive ? "Activated" : "Deactivated")} user {user.UserName}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"User {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }
        [Authorize(Policy = "ActiveUser")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (userId is null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound(new { message = "User not found" });
            if (!user.IsActive) return Unauthorized(new { message = "Account is deactivated" });

            if (dto.NewPassword != dto.ConfirmNewPassword)
                return BadRequest(new { message = "New password and confirmation do not match" });

            if (dto.NewPassword == dto.OldPassword)
                return BadRequest(new { message = "New password must be different from the old password" });

            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var firstError = result.Errors.FirstOrDefault()?.Description ?? "Password change failed";
                return BadRequest(new { message = firstError });
            }

            await _auditLogService.LogAsync(
                User,
                "ChangePassword",
                $"User {user.UserName} changed their password",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Password changed successfully" });
        }
        [Authorize(Roles ="Admin")]
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteSelf()
        {
            var currentUserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (currentUserId is null)
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user is null)
                return NotFound(new { message = "User not found" });

            // Confirm this really is an admin path (defensive check — should always be true given [Authorize(Roles = "Admin")])
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                if (adminUsers.Count <= 1)
                {
                    return BadRequest(new { message = "Cannot delete the last remaining admin account" });
                }
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _auditLogService.LogAsync(
                User,
                "DeleteSelf",
                $"Admin {user.UserName} deleted their own account",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Account deleted successfully" });
        }

    }
}
