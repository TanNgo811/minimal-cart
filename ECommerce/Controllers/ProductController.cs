using ECommerce.DTOs;
using ECommerce.Interfaces;
using ECommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetProducts(
        [FromQuery] ProductFilterDto filter)
    {
        var productListDto = await _productService.GetAllProductsAsync(filter);
        return Ok(productListDto);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto productDto)
    {
        var product = await _productService.CreateProductAsync(productDto);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, UpdateProductDto productDto)
    {
        var product = await _productService.UpdateProductAsync(id, productDto);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var result = await _productService.DeleteProductAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetProductsByCategory(
        Guid categoryId,
        [FromQuery] ProductFilterDto filter)
    {
        var products = await _productService.GetProductsByCategoryAsync(categoryId, filter);
        
        return Ok(products);
    }
}
