using CuraLink.Infrastructure.Persistence;
using CuraLink.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CuraLink.IntegrationTests;

public sealed class CuraLinkWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("SeedDatabase", "false");
        builder.UseSetting("UseHttps", "false");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            var databaseName = $"CuraLinkTests_{Guid.NewGuid():N}";
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName));
        });
    }
}
