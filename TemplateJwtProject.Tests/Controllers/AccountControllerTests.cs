using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TemplateJwtProject.Controllers;
using TemplateJwtProject.Models;
using TemplateJwtProject.Models.DTOs;
using TemplateJwtProject.Tests.Helpers;
using Xunit;

namespace TemplateJwtProject.Tests.Controllers;

public class AccountControllerTests
{
    [Fact]
    public async Task GetProfile_WhenUserMissing_ReturnsNotFound()
    {
        var userManager = IdentityTestHelpers.CreateUserManager();
        var logger = Mock.Of<ILogger<AccountController>>();
        var controller = new AccountController(userManager.Object, logger);
        IdentityTestHelpers.SetUser(controller, new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) });

        var result = await controller.GetProfile();

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdatePassword_WhenModelInvalid_ReturnsBadRequest()
    {
        var userManager = IdentityTestHelpers.CreateUserManager();
        var logger = Mock.Of<ILogger<AccountController>>();
        var controller = new AccountController(userManager.Object, logger);
        controller.ModelState.AddModelError("password", "required");

        var result = await controller.UpdatePassword(new UpdatePasswordDto());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteAccount_WhenPasswordIncorrect_ReturnsBadRequest()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "user@test.com" };
        var users = new List<ApplicationUser> { user };
        var userManager = IdentityTestHelpers.CreateUserManager(users);
        userManager.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);
        var logger = Mock.Of<ILogger<AccountController>>();
        var controller = new AccountController(userManager.Object, logger);
        IdentityTestHelpers.SetUser(controller, new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) });

        var result = await controller.DeleteAccount(new DeleteAccountDto { Password = "wrong" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
