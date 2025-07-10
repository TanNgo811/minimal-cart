using ECommerce.DTOs;

namespace ECommerce.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid userId);
    Task<CartDto> AddToCartAsync(Guid userId, AddToCartDto addToCartDto);
    Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemDto updateCartDto);
    Task<bool> RemoveFromCartAsync(Guid userId, Guid cartItemId);
    Task<bool> ClearCartAsync(Guid userId);
}
