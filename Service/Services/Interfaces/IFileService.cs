using Microsoft.AspNetCore.Http;

namespace Service.Services.Interfaces
{
    public interface IFileService
    {
        // Şəkli yoxlayıb saxlayır, saytda açılan yolu qaytarır (məs. /uploads/products/3f2a....png)
        Task<string> UploadImageAsync(IFormFile file, string folder);

        // UploadImageAsync-in qaytardığı yolu verirsən. Yalnız bu sistemin yüklədiyi şəkli silir
        Task DeleteFileAsync(string webPath);
    }
}
