public class ProductStatusTests
{
    [Theory]
    [InlineData(0, StockStatus.OutOfStock)]
    [InlineData(1, StockStatus.LowStock)]
    [InlineData(10, StockStatus.LowStock)]
    [InlineData(11, StockStatus.Normal)]
    [InlineData(173, StockStatus.Normal)]
    public void Status_FollowsStockThresholds(int stock, StockStatus expected)
    {
        var product = new Product { Stock = stock };

        Assert.Equal(expected, product.Status);
    }
}