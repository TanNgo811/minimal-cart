using ECommerce.Data;
using ECommerce.DTOs;
using ECommerce.Interfaces;
using ECommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductListDto> GetAllProductsAsync(ProductFilterDto filter)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Where(p => (string.IsNullOrEmpty(filter.Search) || p.Name.Contains(filter.Search) || p.Description.Contains(filter.Search)) &&
                        (filter.CategoryIds == null || filter.CategoryIds.Count == 0 || filter.CategoryIds.Contains(p.CategoryId)) &&
                        (filter.MinPrice == null || p.Price >= filter.MinPrice) &&
                        (filter.MaxPrice == null || p.Price <= filter.MaxPrice) &&
                        (filter.MinAmount == null || p.StockQuantity >= filter.MinAmount) &&
                        (filter.MaxAmount == null || p.StockQuantity <= filter.MaxAmount) &&
                        p.IsActive == filter.IsActive);

        query = filter.SortBy switch
        {
            ProductSortOption.PriceAsc => query.OrderBy(p => p.Price),
            ProductSortOption.PriceDesc => query.OrderByDescending(p => p.Price),
            ProductSortOption.NameAsc => query.OrderBy(p => p.Name),
            ProductSortOption.NameDesc => query.OrderByDescending(p => p.Name),
            ProductSortOption.CreatedAtDesc => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        // Execute count query first
        var totalCount = await _context.Products
            .Where(p => (string.IsNullOrEmpty(filter.Search) || p.Name.Contains(filter.Search) || p.Description.Contains(filter.Search)) &&
                        (filter.CategoryIds == null || filter.CategoryIds.Count == 0 || filter.CategoryIds.Contains(p.CategoryId)) &&
                        (filter.MinPrice == null || p.Price >= filter.MinPrice) &&
                        (filter.MaxPrice == null || p.Price <= filter.MaxPrice) &&
                        (filter.MinAmount == null || p.StockQuantity >= filter.MinAmount) &&
                        (filter.MaxAmount == null || p.StockQuantity <= filter.MaxAmount) &&
                        p.IsActive == filter.IsActive)
            .CountAsync();

        // Then execute the products query
        var products = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.StockQuantity,
                p.ImageUrl,
                p.IsActive,
                p.CategoryId,
                p.Category.Name
            ))
            .ToListAsync();
        
        return new ProductListDto(products, totalCount);
    }

    public async Task<IEnumerable<ProductListDto>> GetProductsByCategoryAsync(Guid categoryId, ProductFilterDto filter)
    {
        if (filter.CategoryIds == null)
        {
            filter.CategoryIds = new List<Guid>();
        }
        filter.CategoryIds.Add(categoryId);
        var result = await GetAllProductsAsync(filter);
        return new List<ProductListDto> { result };
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product == null ? null : new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.ImageUrl,
            product.IsActive,
            product.CategoryId,
            product.Category.Name);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
    {
        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            StockQuantity = productDto.StockQuantity,
            ImageUrl = productDto.ImageUrl,
            CategoryId = productDto.CategoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var category = await _context.Categories.FindAsync(product.CategoryId);
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.ImageUrl,
            product.IsActive,
            product.CategoryId,
            category?.Name ?? "Unknown");
    }

    public async Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto productDto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return null;

        product.Name = productDto.Name;
        product.Description = productDto.Description;
        product.Price = productDto.Price;
        product.StockQuantity = productDto.StockQuantity;
        product.ImageUrl = productDto.ImageUrl;
        product.IsActive = productDto.IsActive;
        product.CategoryId = productDto.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var category = await _context.Categories.FindAsync(product.CategoryId);
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.ImageUrl,
            product.IsActive,
            product.CategoryId,
            category?.Name ?? "Unknown");
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}
