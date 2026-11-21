using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);

            e.Property(p => p.Code).IsRequired().HasMaxLength(20);
            e.HasIndex(p => p.Code).IsUnique();

            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.Property(p => p.Category).IsRequired().HasMaxLength(50);
            e.Property(p => p.Price).HasPrecision(18, 2);

            e.Ignore(p => p.Status);

            e.HasData(
                new Product { Id = 1, Code = "IT-001", Name = "無線鍵盤",          Category = "電腦周邊", Price = 1290, Stock = 42 },
                new Product { Id = 2, Code = "IT-002", Name = "USB-C 集線器",      Category = "電腦周邊", Price = 890,  Stock = 5  },
                new Product { Id = 3, Code = "NW-001", Name = "Gigabit 網路交換器", Category = "網路設備", Price = 1690, Stock = 18 },
                new Product { Id = 4, Code = "NW-002", Name = "Cat 6 網路線",       Category = "網路設備", Price = 150,  Stock = 96 },
                new Product { Id = 5, Code = "ST-001", Name = "NVMe SSD 1TB",      Category = "儲存設備", Price = 2490, Stock = 0  },
                new Product { Id = 6, Code = "ST-002", Name = "外接硬碟 2TB",       Category = "儲存設備", Price = 2190, Stock = 12 }
            );
        });
    }
}