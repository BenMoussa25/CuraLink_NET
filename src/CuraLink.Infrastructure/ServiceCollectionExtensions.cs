using CuraLink.Application.Interfaces;
using CuraLink.Infrastructure.Email;
using CuraLink.Infrastructure.FileStorage;
using CuraLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CuraLink.Infrastructure;

public static class ServiceCollectionExtensions
{
    private const string DefaultConnectionString = "Server=(localdb)\\mssqllocaldb;Database=CuraLinkDb;Trusted_Connection=True;TrustServerCertificate=True";

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? DefaultConnectionString;

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IEmailSender, DevelopmentEmailSender>();

        return services;
    }
}
