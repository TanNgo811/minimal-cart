using ECommerce.DTOs;
using ECommerce.Models;

namespace ECommerce.Interfaces;

public interface IProductService
{
    Task<ProductListDto> GetAllProductsAsync(ProductFilterDto filter);
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<ProductDto> CreateProductAsync(CreateProductDto productDto);
    Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto productDto);
    Task<bool> DeleteProductAsync(Guid id);
    Task<IEnumerable<ProductListDto>> GetProductsByCategoryAsync(Guid categoryId, ProductFilterDto filter);
}
