using System.Security.Claims;
using ECommerce.DTOs;
using ECommerce.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers;

[ApiController]
[Route("api/products/{productId}/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<ActionResult<ProductReviewSummaryDto>> GetProductReviews(Guid productId)
    {
        var reviews = await _reviewService.GetProductReviewsAsync(productId);
        return Ok(reviews);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> CreateReview(Guid productId, CreateReviewDto reviewDto)
    {
        try
        {
            var userId = GetUserId();
            var review = await _reviewService.CreateReviewAsync(productId, userId, reviewDto);
            return CreatedAtAction(nameof(GetProductReviews), new { productId }, review);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{reviewId:guid}")]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> UpdateReview(Guid productId, Guid reviewId, UpdateReviewDto reviewDto)
    {
        try
        {
            var userId = GetUserId();
            var review = await _reviewService.UpdateReviewAsync(reviewId, userId, reviewDto);
            
            if (review == null)
                return NotFound();

            return Ok(review);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{reviewId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(Guid productId, Guid reviewId)
    {
        var userId = GetUserId();
        var result = await _reviewService.DeleteReviewAsync(reviewId, userId);
        
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetUserReviews()
    {
        var userId = GetUserId();
        var reviews = await _reviewService.GetUserReviewsAsync(userId);
        return Ok(reviews);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID");
        }
        return userId;
    }
}
