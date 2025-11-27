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
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameUk = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DescriptionUk = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    DiscountPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    IsPromotional = table.Column<bool>(type: "boolean", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    SubCategory = table.Column<string>(type: "text", nullable: true),
                    SubCategoryUk = table.Column<string>(type: "text", nullable: true),
                    IsVegetarian = table.Column<bool>(type: "boolean", nullable: false),
                    Ingredients = table.Column<string>(type: "text", nullable: false),
                    IngredientsUk = table.Column<string>(type: "text", nullable: false),
                    Allergens = table.Column<string>(type: "text", nullable: false),
                    AllergensUk = table.Column<string>(type: "text", nullable: false),
                    Photos = table.Column<string>(type: "text", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    Rating = table.Column<double>(type: "double precision", nullable: false),
                    RatingCount = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HookahDetails_TobaccoFlavor = table.Column<string>(type: "text", nullable: true),
                    HookahDetails_TobaccoFlavorUk = table.Column<string>(type: "text", nullable: true),
                    HookahDetails_Strength = table.Column<string>(type: "text", nullable: true),
                    HookahDetails_BowlType = table.Column<string>(type: "text", nullable: true),
                    HookahDetails_BowlTypeUk = table.Column<string>(type: "text", nullable: true),
                    HookahDetails_AdditionalParams = table.Column<string>(type: "text", nullable: true),
                    HookahDetails_AdditionalParamsUk = table.Column<string>(type: "text", nullable: true)
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
