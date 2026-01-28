using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TemplateJwtProject.Models;
using TemplateJwtProject.Models.DTOs;

namespace TemplateJwtProject.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    // Gets the current user from the database using JWT claims
    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        // Try multiple ways to get the user ID (GUID)
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid")
            ?? User.FindFirstValue("sub");

        if (!string.IsNullOrEmpty(userId))
        {
            // First try as GUID (user ID)
            if (Guid.TryParse(userId, out _))
            {
                var userById = await _userManager.FindByIdAsync(userId);
                if (userById != null)
                    return userById;
            }
            
            // Try as email
            var userByEmail = await _userManager.FindByEmailAsync(userId);
            if (userByEmail != null)
                return userByEmail;
        }

        // Try the email claim directly
        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email");
        
        if (!string.IsNullOrEmpty(email))
        {
            var userByEmail = await _userManager.FindByEmailAsync(email);
            if (userByEmail != null)
                return userByEmail;
        }

        return null;
    }

    // GET /api/account/profile - Returns user profile info
    [HttpGet("profile")]
    public async Task<ActionResult<AccountDto>> GetProfile()
    {
        var user = await GetCurrentUserAsync();
        
        if (user == null)
        {
            return NotFound(new { message = "User not found in database" });
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new AccountDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList()
        });
    }

    // PUT /api/account/password - Change user password
    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return NotFound(new { message = "User not found in database" });
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        _logger.LogInformation("User {UserId} changed their password", user.Id);

        return Ok(new { message = "Password updated successfully" });
    }

    // DELETE /api/account - Delete user account (requires password confirmation)
    [HttpDelete]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return NotFound(new { message = "User not found in database" });
        }

        // Verify password before deletion
        var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!passwordValid)
        {
            return BadRequest(new { message = "Password is incorrect" });
        }

        var userId = user.Id;
        var userEmail = user.Email;

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        _logger.LogInformation("User {UserId} ({Email}) deleted their account", userId, userEmail);

        return Ok(new { message = "Account deleted successfully" });
    }
}
