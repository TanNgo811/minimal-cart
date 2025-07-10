using ECommerce.DTOs;

namespace ECommerce.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> CreateReviewAsync(Guid productId, Guid userId, CreateReviewDto reviewDto);
    Task<ReviewDto?> UpdateReviewAsync(Guid reviewId, Guid userId, UpdateReviewDto reviewDto);
    Task<bool> DeleteReviewAsync(Guid reviewId, Guid userId);
    Task<ProductReviewSummaryDto> GetProductReviewsAsync(Guid productId);
    Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(Guid userId);
    Task<bool> HasUserReviewedProductAsync(Guid productId, Guid userId);
}
