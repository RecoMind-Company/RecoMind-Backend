namespace Core.Service.Interface;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(string content);
    Task<string> ReadFileAsync(string path);
}
