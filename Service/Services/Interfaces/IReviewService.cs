using Service.Helpers.DTOs.Reviews;

namespace Service.Services.Interfaces
{
    public interface IReviewService
    {
        // userId boş ola bilər (daxil olmayan istifadəçi): onda CanReview və MyReview boşdur
        Task<ProductReviewsDto> GetForProductAsync(int productId, string? userId);
        Task<ProductReviewDto> SaveForProductAsync(int productId, string userId, SaveProductReviewDto model);
        Task DeleteForProductAsync(int productId, string userId);
    }
}
