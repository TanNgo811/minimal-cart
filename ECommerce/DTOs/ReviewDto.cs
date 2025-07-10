namespace ECommerce.DTOs;

public record ReviewDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid UserId,
    string Username,
    int Rating,
    string Comment,
    DateTime CreatedAt
);

public record CreateReviewDto(
    int Rating,
    string Comment
);

public record UpdateReviewDto(
    int Rating,
    string Comment
);

public record ProductReviewSummaryDto(
    double AverageRating,
    int TotalReviews,
    IEnumerable<ReviewDto> Reviews
);
