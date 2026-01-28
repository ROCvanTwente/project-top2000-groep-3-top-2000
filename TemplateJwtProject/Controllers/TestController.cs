using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TemplateJwtProject.Constants;
using TemplateJwtProject.Data;

namespace TemplateJwtProject.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class TestController : ControllerBase
{
    private readonly AppDbContext _context;

    public TestController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            message = "API is working",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("user")]
    [Authorize(Roles = Roles.User)]
    public IActionResult UserEndpoint()
    {
        return Ok(new { message = "This endpoint is accessible by Users", user = User.Identity?.Name });
    }

    [HttpGet("admin")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult AdminEndpoint()
    {
        return Ok(new { message = "This endpoint is only accessible by Admins", user = User.Identity?.Name });
    }

    [HttpGet("user-or-admin")]
    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    public IActionResult UserOrAdminEndpoint()
    {
        var roles = User.Claims
            .Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(c => c.Value)
            .ToList();

        return Ok(new
        {
            message = "This endpoint is accessible by Users or Admins",
            user = User.Identity?.Name,
            roles = roles
        });
    }

    [HttpGet("verify-user")]
    [Authorize]
    public async Task<IActionResult> VerifyUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { message = "No user ID in JWT" });
        }

        var userExists = await _context.Users.FindAsync(userId);
        var userByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        return Ok(new
        {
            jwtUserId = userId,
            jwtEmail = email,
            userExistsById = userExists != null,
            userExistsByEmail = userByEmail != null,
            userInDatabase = userExists != null ? new
            {
                id = userExists.Id,
                email = userExists.Email,
                userName = userExists.UserName
            } : null
        });
    }
}
