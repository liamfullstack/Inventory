using Microsoft.EntityFrameworkCore;

public class ProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db) => _db = db;

    public async Task<PagedResult<ProductDto>> GetListAsync(
        string? keyword, string? category, StockStatus? status, int page = 1, int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(p => p.Name.Contains(keyword) || p.Code.Contains(keyword));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        query = status switch
        {
            StockStatus.OutOfStock => query.Where(p => p.Stock == 0),
            StockStatus.LowStock   => query.Where(p => p.Stock >= 1 && p.Stock <= 10),
            StockStatus.Normal     => query.Where(p => p.Stock > 10),
            _ => query
        };

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ProductDto>(items.Select(ProductDto.From).ToList(), total, page, pageSize);
    }

    public async Task<ProductSummary> GetSummaryAsync()
    {
        var rows = await _db.Products.AsNoTracking()
            .Select(p => new { p.Price, p.Stock })
            .ToListAsync();

        return new ProductSummary(
            rows.Count,
            rows.Sum(r => r.Stock),
            rows.Sum(r => r.Price * r.Stock),
            rows.Count(r => r.Stock <= 10));
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var p = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? null : ProductDto.From(p);
    }

    public async Task<ProductDto> CreateAsync(ProductRequest req)
    {
        var p = new Product
        {
            Code = req.Code, Name = req.Name, Category = req.Category,
            Price = req.Price, Stock = req.Stock
        };
        _db.Products.Add(p);
        await _db.SaveChangesAsync();
        return ProductDto.From(p);
    }

    public async Task<ProductDto?> UpdateAsync(int id, ProductRequest req)
    {
        var p = await _db.Products.FindAsync(id);
        if (p is null) return null;

        p.Code = req.Code;
        p.Name = req.Name;
        p.Category = req.Category;
        p.Price = req.Price;
        p.Stock = req.Stock;

        await _db.SaveChangesAsync();
        return ProductDto.From(p);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p is null) return false;

        _db.Products.Remove(p);
        await _db.SaveChangesAsync();
        return true;
    }
}