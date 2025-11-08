using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KosherClouds.ProductService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    SubCategory = table.Column<string>(type: "text", nullable: true),
                    Ingredients = table.Column<string>(type: "text", nullable: false),
                    Allergens = table.Column<string>(type: "text", nullable: false),
                    Photos = table.Column<string>(type: "text", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    Rating = table.Column<double>(type: "double precision", nullable: false),
                    RatingCount = table.Column<long>(type: "bigint", nullable: false),
                    HookahDetails_TobaccoFlavor = table.Column<string>(type: "text", nullable: true),
                    HookahDetails_Strength = table.Column<string>(type: "text", nullable: true),
                    HookahDetails_BowlType = table.Column<string>(type: "text", nullable: true),
                    HookahDetails_AdditionalParams = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
