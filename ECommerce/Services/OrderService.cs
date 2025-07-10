using ECommerce.Data;
using ECommerce.DTOs;
using ECommerce.Interfaces;
using ECommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto orderDto)
    {
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            ShippingAddress = orderDto.ShippingAddress,
            PaymentMethod = orderDto.PaymentMethod,
            PaymentStatus = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Calculate order items and total
        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in orderDto.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId)
                ?? throw new InvalidOperationException($"Product not found: {item.ProductId}");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Insufficient stock for product: {product.Name}");

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                Subtotal = product.Price * item.Quantity
            };

            // Update product stock
            product.StockQuantity -= item.Quantity;
            orderItems.Add(orderItem);
            totalAmount += orderItem.Subtotal;
        }

        order.TotalAmount = totalAmount;
        order.OrderItems = orderItems;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(order.Id, userId) 
            ?? throw new InvalidOperationException("Failed to create order");
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid userId)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && (o.UserId == userId));

        if (order == null) return null;

        return MapOrderToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(MapOrderToDto);
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return null;

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapOrderToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(MapOrderToDto);
    }

    public async Task<bool> CancelOrderAsync(Guid orderId, Guid userId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null || order.Status != OrderStatus.Pending)
            return false;

        // Restore product stock
        foreach (var item in order.OrderItems)
        {
            item.Product.StockQuantity += item.Quantity;
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private static OrderDto MapOrderToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.UserId,
            order.User.Email,
            order.OrderDate,
            order.Status,
            order.TotalAmount,
            order.ShippingAddress,
            order.PaymentMethod,
            order.PaymentStatus,
            order.OrderItems.Select(oi => new OrderItemDto(
                oi.Id,
                oi.ProductId,
                oi.Product.Name,
                oi.Quantity,
                oi.UnitPrice,
                oi.Subtotal
            ))
        );
    }
}
