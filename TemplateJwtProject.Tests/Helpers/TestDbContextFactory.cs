using Microsoft.EntityFrameworkCore;
using TemplateJwtProject.Data;

namespace TemplateJwtProject.Tests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext CreateContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName ?? System.Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
