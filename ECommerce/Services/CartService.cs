using ECommerce.Data;
using ECommerce.DTOs;
using ECommerce.Interfaces;
using ECommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> GetCartAsync(Guid userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return await MapCartToDtoAsync(cart);
    }

    public async Task<CartDto> AddToCartAsync(Guid userId, AddToCartDto addToCartDto)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var product = await _context.Products.FindAsync(addToCartDto.ProductId)
            ?? throw new InvalidOperationException("Product not found");

        if (product.StockQuantity < addToCartDto.Quantity)
            throw new InvalidOperationException("Insufficient stock");

        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == addToCartDto.ProductId);

        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + addToCartDto.Quantity;
            if (product.StockQuantity < newQuantity)
                throw new InvalidOperationException("Insufficient stock");

            existingItem.Quantity = newQuantity;
            existingItem.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = addToCartDto.Quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CartItems.Add(cartItem);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return await MapCartToDtoAsync(cart);
    }

    public async Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemDto updateCartDto)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var cartItem = await _context.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.CartId == cart.Id)
            ?? throw new InvalidOperationException("Cart item not found");

        if (updateCartDto.Quantity <= 0)
        {
            _context.CartItems.Remove(cartItem);
        }
        else
        {
            if (cartItem.Product.StockQuantity < updateCartDto.Quantity)
                throw new InvalidOperationException("Insufficient stock");

            cartItem.Quantity = updateCartDto.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return await MapCartToDtoAsync(cart);
    }

    public async Task<bool> RemoveFromCartAsync(Guid userId, Guid cartItemId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.CartId == cart.Id);

        if (cartItem == null)
            return false;

        _context.CartItems.Remove(cartItem);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ClearCartAsync(Guid userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var cartItems = await _context.CartItems
            .Where(ci => ci.CartId == cart.Id)
            .ToListAsync();

        _context.CartItems.RemoveRange(cartItems);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return true;
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart != null)
            return cart;

        cart = new Cart
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        
        return cart;
    }

    private static async Task<CartDto> MapCartToDtoAsync(Cart cart)
    {
        var items = cart.Items.Select(ci => new CartItemDto(
            ci.Id,
            ci.ProductId,
            ci.Product.Name,
            ci.Product.ImageUrl,
            ci.Product.Price,
            ci.Quantity,
            ci.Product.Price * ci.Quantity
        ));

        var totalAmount = items.Sum(i => i.Subtotal);

        return new CartDto(
            cart.Id,
            cart.UserId,
            items,
            totalAmount
        );
    }
}
