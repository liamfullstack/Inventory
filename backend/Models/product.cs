public enum StockStatus
{
    LowStock,
    OutOfStock,
    Normal
}


public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
     public int Stock { get; set; }                 // 庫存數量(原本第 16 行改成這個)


    public StockStatus Status => Stock switch
    {
        0 => StockStatus.OutOfStock,
        <= 10 => StockStatus.LowStock,
        _ => StockStatus.Normal
    };
}