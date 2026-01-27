using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TemplateJwtProject.Constants;
using TemplateJwtProject.Controllers;
using TemplateJwtProject.Models;
using TemplateJwtProject.Models.DTOs;
using TemplateJwtProject.Tests.Helpers;
using Xunit;

namespace TemplateJwtProject.Tests.Controllers;

public class AdminControllerTests
{
    [Fact]
    public async Task AssignRole_WhenUserMissing_ReturnsNotFound()
    {
        var userManager = IdentityTestHelpers.CreateUserManager();
        userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        var logger = Mock.Of<ILogger<AdminController>>();
        var controller = new AdminController(userManager.Object, logger);

        var result = await controller.AssignRole(new AssignRoleDto { Email = "missing@test.com", Role = Roles.User });

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RemoveRole_WhenUserDoesNotHaveRole_ReturnsBadRequest()
    {
        var user = new ApplicationUser { Id = System.Guid.NewGuid().ToString(), Email = "user@test.com" };
        var userManager = IdentityTestHelpers.CreateUserManager(new List<ApplicationUser> { user });
        userManager.Setup(m => m.IsInRoleAsync(user, Roles.Admin)).ReturnsAsync(false);
        var logger = Mock.Of<ILogger<AdminController>>();
        var controller = new AdminController(userManager.Object, logger);

        var result = await controller.RemoveRole(new AssignRoleDto { Email = user.Email!, Role = Roles.Admin });

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
