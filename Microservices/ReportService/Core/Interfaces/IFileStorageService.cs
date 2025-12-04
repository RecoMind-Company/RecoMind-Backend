namespace Core.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(string content);
}
