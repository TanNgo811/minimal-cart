using ECommerce.DTOs;
using ECommerce.Models;

namespace ECommerce.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto orderDto);
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid userId);
    Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId);
    Task<OrderDto?> UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<bool> CancelOrderAsync(Guid orderId, Guid userId);
}
