using Azure.Storage.Blobs;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Infrastructure.FileStorage
{
    public class AzureFileStorage : IFileStorageService
    {
        private readonly BlobContainerClient _containerClient;
        public AzureFileStorage(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            var containerName = configuration.GetValue<string>("BlobContainers:Reports");

            _containerClient = new BlobContainerClient(connectionString, containerName);
        }
        public async Task<string> ReadFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            // الحصول على reference للملف باسمه
            BlobClient blobClient = _containerClient.GetBlobClient(fileName);

            // التأكد إن الملف موجود على Azure فعلاً
            if (!await blobClient.ExistsAsync())
                return string.Empty;

            // تحميل محتوى الملف
            var downloadInfo = await blobClient.DownloadContentAsync();

            // تحويل المحتوى لـ String ورجوعه
            return downloadInfo.Value.Content.ToString();
        }

        public async Task<string> SaveFileAsync(string content)
        {
            // نأمن إن الـ Container موجود على Azure (بيعمل Check أول مرة بس لو مش موجود بيكريته)
            await _containerClient.CreateIfNotExistsAsync();

            // إنشاء اسم فريد للملف
            var fileName = $"Report_{Guid.NewGuid()}.txt";

            // الحصول على reference للملف جوه Azure
            BlobClient blobClient = _containerClient.GetBlobClient(fileName);

            // تحويل الـ string لـ Stream عشان ينفع نرفعه
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // رفع الملف لـ Azure Blob Storage
            await blobClient.UploadAsync(memoryStream, overwrite: true);

            // هنا بنرجع اسم الملف نفسه (هو ده الـ dynamic path الجديد بتاعنا)
            return fileName;
        }
    }
}

