namespace ECommerce.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}

public enum ProductSortOption
{
    PriceAsc,
    PriceDesc,
    NameAsc,
    NameDesc,
    CreatedAtDesc,
}