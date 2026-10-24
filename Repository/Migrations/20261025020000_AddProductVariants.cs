using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CartItems_UserId_ProductId",
                table: "CartItems");

            migrationBuilder.AddColumn<string>(
                name: "BrewMethods",
                table: "Products",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Grind",
                table: "OrderItems",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantLabel",
                table: "OrderItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Grind",
                table: "CartItems",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Format = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Size = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OldPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariantImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariantId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariantImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariantImages_ProductVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_VariantId",
                table: "OrderItems",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserId_ProductId",
                table: "CartItems",
                columns: new[] { "UserId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_VariantId",
                table: "CartItems",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantImages_VariantId",
                table: "ProductVariantImages",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ProductVariants_VariantId",
                table: "CartItems",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductVariants_VariantId",
                table: "OrderItems",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_ProductVariants_VariantId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductVariants_VariantId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "ProductVariantImages");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_VariantId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_UserId_ProductId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_VariantId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "BrewMethods",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Grind",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VariantLabel",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Grind",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "CartItems");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserId_ProductId",
                table: "CartItems",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }
    }
}
