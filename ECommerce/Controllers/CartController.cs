using System.Security.Claims;
using ECommerce.DTOs;
using ECommerce.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = GetUserId();
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddToCart(AddToCartDto addToCartDto)
    {
        var userId = GetUserId();
        try
        {
            var cart = await _cartService.AddToCartAsync(userId, addToCartDto);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("items/{id:guid}")]
    public async Task<ActionResult<CartDto>> UpdateCartItem(Guid id, UpdateCartItemDto updateCartDto)
    {
        var userId = GetUserId();
        try
        {
            var cart = await _cartService.UpdateCartItemAsync(userId, id, updateCartDto);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> RemoveFromCart(Guid id)
    {
        var userId = GetUserId();
        var result = await _cartService.RemoveFromCartAsync(userId, id);
        
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        await _cartService.ClearCartAsync(userId);
        return NoContent();
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
