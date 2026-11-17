namespace CuraLink.Application.Interfaces;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string originalFileName, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
    void Delete(string storageKey);
}
