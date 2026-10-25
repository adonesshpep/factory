using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

public class ActiveUserHandler : AuthorizationHandler<ActiveUserRequirement>
{
    private readonly UserManager<AppUser> _userManager;

    public ActiveUserHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, ActiveUserRequirement requirement)
    {
        var userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userId is null)
        {
            context.Fail();
            return;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || !user.IsActive)
        {
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }
}