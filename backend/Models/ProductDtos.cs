using System.ComponentModel.DataAnnotations;


public record ProductDto(int Id, string Code, string Name, string Category, decimal Price, int Stock, StockStatus Status)
{
    public static ProductDto From(Product p) =>
        new(p.Id, p.Code, p.Name, p.Category, p.Price, p.Stock, p.Status);
}

public record ProductRequest(
    [Required, StringLength(20)] string Code,
    [Required, StringLength(100)] string Name,
    [Required, StringLength(50)] string Category,
    [Range(0, 99999999)] decimal Price,
    [Range(0, int.MaxValue)] int Stock);

public record ProductSummary(int TotalTypes, int TotalStock, decimal TotalValue, int ReplenishCount);

public record PagedResult<T>(List<T> Items, int Total, int Page, int PageSize);