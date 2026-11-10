using Microsoft.AspNetCore.Http;
using Service.Services.Interfaces;

namespace Janz.Tests.Fakes
{
    // Fayl yazmır: yüklənən və silinən yolları yadda saxlayır
    public class FakeFileService : IFileService
    {
        public List<string> Uploaded { get; } = new();
        public List<string> Deleted { get; } = new();

        public Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            var path = $"/uploads/{folder}/new-{Uploaded.Count + 1}.png";
            Uploaded.Add(path);
            return Task.FromResult(path);
        }

        public Task DeleteFileAsync(string webPath)
        {
            Deleted.Add(webPath);
            return Task.CompletedTask;
        }
    }
}
