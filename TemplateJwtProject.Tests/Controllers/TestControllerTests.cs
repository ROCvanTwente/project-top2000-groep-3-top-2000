using System.Collections.Generic;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TemplateJwtProject.Constants;
using TemplateJwtProject.Controllers;
using TemplateJwtProject.Tests.Helpers;
using Xunit;

namespace TemplateJwtProject.Tests.Controllers;

public class TestControllerTests
{
    [Fact]
    public void Get_WhenCalled_ReturnsOk()
    {
        var controller = new TestController();

        var result = controller.Get();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void UserEndpoint_WithUserRole_ReturnsOk()
    {
        var controller = new TestController();
        IdentityTestHelpers.SetUser(controller, new[]
        {
            new Claim(ClaimTypes.Role, Roles.User),
            new Claim(ClaimTypes.Name, "user@test.com")
        });

        var result = controller.UserEndpoint();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void AdminEndpoint_WithAdminRole_ReturnsOk()
    {
        var controller = new TestController();
        IdentityTestHelpers.SetUser(controller, new[]
        {
            new Claim(ClaimTypes.Role, Roles.Admin),
            new Claim(ClaimTypes.Name, "admin@test.com")
        });

        var result = controller.AdminEndpoint();

        result.Should().BeOfType<OkObjectResult>();
    }
}
