namespace ECommerce.DTOs;

public record CartDto(
    Guid Id,
    Guid UserId,
    IEnumerable<CartItemDto> Items,
    decimal TotalAmount
);

public record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal
);

public record AddToCartDto(
    Guid ProductId,
    int Quantity
);

public record UpdateCartItemDto(
    int Quantity
);
