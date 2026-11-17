using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CuraLink.IntegrationTests;

public class HealthAndAuthorizationTests : IClassFixture<CuraLinkWebApplicationFactory>
{
    private readonly CuraLinkWebApplicationFactory _factory;

    public HealthAndAuthorizationTests(CuraLinkWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("ok", body);
    }

    [Fact]
    public async Task GetDoctorDashboard_WhenUnauthenticated_RedirectsToLogin()
    {
        var options = new WebApplicationFactoryClientOptions { AllowAutoRedirect = false };
        using var client = _factory.CreateClient(options);

        var response = await client.GetAsync("/Doctor/Dashboard");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/Account/Login", response.Headers.Location!.ToString());
    }
}
