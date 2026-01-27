using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TemplateJwtProject.Constants;
using TemplateJwtProject.Controllers;
using TemplateJwtProject.Models;
using TemplateJwtProject.Models.DTOs;
using TemplateJwtProject.Services;
using TemplateJwtProject.Tests.Helpers;
using Xunit;

namespace TemplateJwtProject.Tests.Controllers;

public class AuthControllerTests
{
    private static AuthController BuildController(
        Mock<UserManager<ApplicationUser>>? userManagerMock = null,
        Mock<SignInManager<ApplicationUser>>? signInManagerMock = null,
        Mock<RoleManager<IdentityRole>>? roleManagerMock = null,
        Mock<IJwtService>? jwtServiceMock = null,
        Mock<IRefreshTokenService>? refreshTokenServiceMock = null)
    {
        var userManager = userManagerMock ?? IdentityTestHelpers.CreateUserManager();
        var signInManager = signInManagerMock ?? IdentityTestHelpers.CreateSignInManager(userManager);
        var roleManager = roleManagerMock ?? IdentityTestHelpers.CreateRoleManager();
        var jwtService = jwtServiceMock ?? new Mock<IJwtService>();
        var refreshTokenService = refreshTokenServiceMock ?? new Mock<IRefreshTokenService>();
        var logger = Mock.Of<ILogger<AuthController>>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Jwt:ExpiryInMinutes", "15"},
                {"Jwt:Key", "key"},
                {"Jwt:Issuer", "issuer"},
                {"Jwt:Audience", "audience"}
            })
            .Build();

        return new AuthController(
            userManager.Object,
            signInManager.Object,
            roleManager.Object,
            jwtService.Object,
            refreshTokenService.Object,
            logger,
            configuration);
    }

    [Fact]
    public async Task Register_WhenModelInvalid_ReturnsBadRequest()
    {
        var controller = BuildController();
        controller.ModelState.AddModelError("Email", "Required");

        var result = await controller.Register(new RegisterDto());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_WhenUserNotFound_ReturnsUnauthorized()
    {
        var userManager = IdentityTestHelpers.CreateUserManager();
        userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        var controller = BuildController(userManagerMock: userManager);

        var result = await controller.Login(new LoginDto { Email = "missing@test.com", Password = "pwd" });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task RefreshToken_WhenInvalid_ReturnsUnauthorized()
    {
        var refreshTokenService = new Mock<IRefreshTokenService>();
        refreshTokenService.Setup(s => s.ValidateRefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((TemplateJwtProject.Models.RefreshToken?)null);

        var controller = BuildController(refreshTokenServiceMock: refreshTokenService);

        var result = await controller.RefreshToken(new RefreshTokenDto { RefreshToken = "bad" });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
