using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class ProductServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        // 記憶體資料庫只活在連線開著的期間,所以連線要保持開啟
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        // 清掉 HasData 的種子資料,改用測試專用的固定資料,避免測試依賴種子內容
        _db.Products.ExecuteDelete();
        _db.Products.AddRange(
            new Product { Code = "A-1", Name = "Alpha", Category = "X", Price = 100, Stock = 0 },
            new Product { Code = "B-1", Name = "Beta",  Category = "X", Price = 200, Stock = 5 },
            new Product { Code = "C-1", Name = "Gamma", Category = "Y", Price = 300, Stock = 10 },
            new Product { Code = "D-1", Name = "Delta", Category = "Y", Price = 400, Stock = 11 });
        _db.SaveChanges();

        _service = new ProductService(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetSummaryAsync_ReturnsCorrectTotals()
    {
        var summary = await _service.GetSummaryAsync();

        Assert.Equal(4, summary.TotalTypes);
        Assert.Equal(26, summary.TotalStock);          // 0 + 5 + 10 + 11
        Assert.Equal(8400m, summary.TotalValue);       // 0*100 + 5*200 + 10*300 + 11*400
        Assert.Equal(3, summary.ReplenishCount);       // 庫存 <= 10 的有 A、B、C
    }

    [Theory]
    [InlineData(StockStatus.OutOfStock, 1)]
    [InlineData(StockStatus.LowStock, 2)]
    [InlineData(StockStatus.Normal, 1)]
    public async Task GetListAsync_FiltersByStatus(StockStatus status, int expectedCount)
    {
        var result = await _service.GetListAsync(null, null, status);

        Assert.Equal(expectedCount, result.Total);
        Assert.All(result.Items, item => Assert.Equal(status, item.Status));
    }

    [Theory]
    [InlineData("B-1", 1)]     // 用編號找
    [InlineData("Gamma", 1)]   // 用名稱找
    [InlineData("-1", 4)]      // 四個編號都含有
    [InlineData("zzz", 0)]     // 找不到
    public async Task GetListAsync_SearchesByCodeOrName(string keyword, int expectedCount)
    {
        var result = await _service.GetListAsync(keyword, null, null);

        Assert.Equal(expectedCount, result.Total);
    }

    [Fact]
    public async Task CreateAsync_PersistsProduct()
    {
        var created = await _service.CreateAsync(new ProductRequest("E-1", "Echo", "X", 50m, 7));

        Assert.True(created.Id > 0);
        Assert.Equal(StockStatus.LowStock, created.Status);
        Assert.Equal(5, await _db.Products.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_Throws()
    {
        var duplicate = new ProductRequest("A-1", "重複編號", "X", 1m, 1);

        await Assert.ThrowsAsync<DbUpdateException>(() => _service.CreateAsync(duplicate));
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_RemovesProduct()
    {
        var id = _db.Products.First(p => p.Code == "A-1").Id;

        var deleted = await _service.DeleteAsync(id);

        Assert.True(deleted);
        Assert.Equal(3, await _db.Products.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_MissingId_ReturnsFalse()
    {
        Assert.False(await _service.DeleteAsync(99999));
    }

    [Fact]
    public async Task UpdateAsync_MissingId_ReturnsNull()
    {
        var result = await _service.UpdateAsync(99999, new ProductRequest("Z-1", "Zulu", "X", 1m, 1));

        Assert.Null(result);
    }
}