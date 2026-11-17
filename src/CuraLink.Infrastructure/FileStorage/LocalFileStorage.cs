using CuraLink.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CuraLink.Infrastructure.FileStorage;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(IWebHostEnvironment environment, IConfiguration configuration, ILogger<LocalFileStorage> logger)
    {
        var configuredRoot = configuration["FileStorage:PrivateRoot"];
        var root = string.IsNullOrWhiteSpace(configuredRoot)
            ? Path.Combine(environment.ContentRootPath, "App_Data", "private-uploads")
            : Path.IsPathRooted(configuredRoot)
                ? configuredRoot
                : Path.Combine(environment.ContentRootPath, configuredRoot);

        _root = Path.GetFullPath(root);
        _logger = logger;
    }

    public async Task<string> SaveAsync(Stream content, string originalFileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (string.IsNullOrWhiteSpace(originalFileName) || originalFileName.Contains("..") ||
            originalFileName.IndexOfAny(new[] { '/', '\\' }) >= 0)
        {
            throw new ArgumentException("Invalid file name.", nameof(originalFileName));
        }

        var storageKey = $"{Guid.NewGuid():N}{GetSafeExtension(originalFileName)}";
        var fullPath = ResolvePath(storageKey);

        Directory.CreateDirectory(_root);

        await using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous))
        {
            await content.CopyToAsync(stream, cancellationToken);
        }

        _logger.LogInformation("A private file was stored.");
        return storageKey;
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(storageKey);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous);
        return Task.FromResult<Stream?>(stream);
    }

    public void Delete(string storageKey)
    {
        var fullPath = ResolvePath(storageKey);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("A private file was removed.");
        }
    }

    private string ResolvePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || storageKey.Contains("..") ||
            storageKey.IndexOfAny(new[] { '/', '\\' }) >= 0 || Path.IsPathRooted(storageKey))
        {
            throw new ArgumentException("Invalid storage key.", nameof(storageKey));
        }

        var fullPath = Path.GetFullPath(Path.Combine(_root, storageKey));
        var rootPrefix = _root.EndsWith(Path.DirectorySeparatorChar) ? _root : _root + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid storage key.", nameof(storageKey));
        }

        return fullPath;
    }

    private static string GetSafeExtension(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);

        if (extension.Length < 2 || extension.Length > 10)
        {
            return string.Empty;
        }

        foreach (var character in extension.AsSpan(1))
        {
            var isAsciiLetterOrDigit = (character >= 'a' && character <= 'z') ||
                                       (character >= 'A' && character <= 'Z') ||
                                       (character >= '0' && character <= '9');
            if (!isAsciiLetterOrDigit)
            {
                return string.Empty;
            }
        }

        return extension.ToLowerInvariant();
    }
}
