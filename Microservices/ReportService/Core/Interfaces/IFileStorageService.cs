namespace Core.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(string content);
    Task<string> ReadFileAsync(string path);
}
