using ECommerce.Models;

namespace ECommerce.DTOs;

public class ProductFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public ProductSortOption? SortBy { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid>? CategoryIds { get; set; }
    public int? MinPrice { get; set; }
    public int? MaxPrice { get; set; }
    public int? MinAmount { get; set; }
    public int? MaxAmount { get; set; }
}
