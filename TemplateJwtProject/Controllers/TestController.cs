using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TemplateJwtProject.Constants;

namespace TemplateJwtProject.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TestController : ControllerBase
{
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
}
