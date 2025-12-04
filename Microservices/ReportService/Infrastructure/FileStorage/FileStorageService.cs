using Core.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.FileStorage;

public class FileStorageService(IWebHostEnvironment env) : IFileStorageService
{

    public async Task<string> SaveFileAsync(string content)
    {
        // get the full physical path to the "StaticFiles/Reports" directory
        var physicalPath = Path.Combine(env.ContentRootPath, "StaticFiles", "Reports");

        // If the directory does not exist, create it
        if (!Directory.Exists(physicalPath))
            Directory.CreateDirectory(physicalPath);

        // Create the file name with .txt extension 
        var fileName = $"Report_{Guid.NewGuid().ToString()}.txt";
        var filePath = Path.Combine(physicalPath, fileName);

        // Write the content to the file
        await File.WriteAllTextAsync(filePath, content);
        var dynamicPath = Path.Combine("StaticFiles", "Reports", fileName).Replace("\\", "/");

        return dynamicPath; // returning the dynamic path
    }
    public async Task<string> ReadFileAsync(string dynamicPath)
    {
        string rootPath = env.ContentRootPath;
        string fullPath = Path.Combine(rootPath, dynamicPath);
        if (!File.Exists(fullPath))
            return "";
        return await File.ReadAllTextAsync(fullPath);
    }

}
