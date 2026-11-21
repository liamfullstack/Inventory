using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations;

/// <inheritdoc />
public partial class _20261121061351_InitPostgres : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Stock = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
            });

        migrationBuilder.InsertData(
            table: "Products",
            columns: new[] { "Id", "Category", "Code", "Name", "Price", "Stock" },
            values: new object[,]
            {
                { 1, "電腦周邊", "IT-001", "無線鍵盤", 1290m, 42 },
                { 2, "電腦周邊", "IT-002", "USB-C 集線器", 890m, 5 },
                { 3, "網路設備", "NW-001", "Gigabit 網路交換器", 1690m, 18 },
                { 4, "網路設備", "NW-002", "Cat 6 網路線", 150m, 96 },
                { 5, "儲存設備", "ST-001", "NVMe SSD 1TB", 2490m, 0 },
                { 6, "儲存設備", "ST-002", "外接硬碟 2TB", 2190m, 12 }
            });

        migrationBuilder.CreateIndex(
            name: "IX_Products_Code",
            table: "Products",
            column: "Code",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Products");
    }
}
