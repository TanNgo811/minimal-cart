using ECommerce.Models;

namespace ECommerce.DTOs;

public record OrderDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    DateTime OrderDate,
    OrderStatus Status,
    decimal TotalAmount,
    string ShippingAddress,
    string PaymentMethod,
    string PaymentStatus,
    IEnumerable<OrderItemDto> Items
);

public record CreateOrderDto(
    string ShippingAddress,
    string PaymentMethod,
    IEnumerable<CreateOrderItemDto> Items
);

public record UpdateOrderStatusDto(
    OrderStatus Status
);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal
);

public record CreateOrderItemDto(
    Guid ProductId,
    int Quantity
);
