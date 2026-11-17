using CuraLink.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CuraLink.Infrastructure.Email;

public sealed class DevelopmentEmailSender : IEmailSender
{
    private readonly ILogger<DevelopmentEmailSender> _logger;

    public DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("A development email notification was requested.");
        return Task.CompletedTask;
    }
}
