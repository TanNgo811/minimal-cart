namespace ECommerce.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string ImageUrl,
    bool IsActive,
    Guid CategoryId,
    string CategoryName
);

public record ProductListDto(
    IEnumerable<ProductDto> Products,
    int TotalCount
);

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string ImageUrl,
    Guid CategoryId
);

public record UpdateProductDto(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string ImageUrl,
    bool IsActive,
    Guid CategoryId
);
