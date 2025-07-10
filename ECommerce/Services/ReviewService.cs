using ECommerce.Data;
using ECommerce.DTOs;
using ECommerce.Interfaces;
using ECommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;

    public ReviewService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewDto> CreateReviewAsync(Guid productId, Guid userId, CreateReviewDto reviewDto)
    {
        var hasReviewed = await HasUserReviewedProductAsync(productId, userId);
        if (hasReviewed)
        {
            throw new InvalidOperationException("User has already reviewed this product");
        }

        if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
        {
            throw new InvalidOperationException("Rating must be between 1 and 5");
        }

        // Verify the user has purchased the product
        var hasPurchased = await _context.Orders
            .AnyAsync(o => o.UserId == userId && 
                          o.Status == OrderStatus.Delivered && 
                          o.OrderItems.Any(oi => oi.ProductId == productId));

        if (!hasPurchased)
        {
            throw new InvalidOperationException("Can only review purchased products");
        }

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = reviewDto.Rating,
            Comment = reviewDto.Comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Add(review);
        await _context.SaveChangesAsync();

        return await MapReviewToDtoAsync(review);
    }

    public async Task<ReviewDto?> UpdateReviewAsync(Guid reviewId, Guid userId, UpdateReviewDto reviewDto)
    {
        var review = await _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

        if (review == null)
            return null;

        if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
        {
            throw new InvalidOperationException("Rating must be between 1 and 5");
        }

        review.Rating = reviewDto.Rating;
        review.Comment = reviewDto.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await MapReviewToDtoAsync(review);
    }

    public async Task<bool> DeleteReviewAsync(Guid reviewId, Guid userId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

        if (review == null)
            return false;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ProductReviewSummaryDto> GetProductReviewsAsync(Guid productId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var reviewDtos = await Task.WhenAll(reviews.Select(MapReviewToDtoAsync));

        var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

        return new ProductReviewSummaryDto(
            averageRating,
            reviews.Count,
            reviewDtos
        );
    }

    public async Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(Guid userId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return await Task.WhenAll(reviews.Select(MapReviewToDtoAsync));
    }

    public async Task<bool> HasUserReviewedProductAsync(Guid productId, Guid userId)
    {
        return await _context.Reviews
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId);
    }

    private async Task<ReviewDto> MapReviewToDtoAsync(Review review)
    {
        return new ReviewDto(
            review.Id,
            review.ProductId,
            review.Product.Name,
            review.UserId,
            review.User.Username,
            review.Rating,
            review.Comment,
            review.CreatedAt
        );
    }
}
