using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.CloudGames.Cart.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init_Cart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cart");

            migrationBuilder.CreateTable(
                name: "CartCustomer",
                schema: "cart",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsedVoucher = table.Column<bool>(type: "bit", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoucherPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VoucherDiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VoucherCode = table.Column<string>(type: "varchar(50)", nullable: true),
                    VoucherDiscountType = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartCustomer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                schema: "cart",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Image = table.Column<string>(type: "varchar(100)", nullable: false),
                    CartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_CartCustomer_CartId",
                        column: x => x.CartId,
                        principalSchema: "cart",
                        principalTable: "CartCustomer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_Customer",
                schema: "cart",
                table: "CartCustomer",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                schema: "cart",
                table: "CartItems",
                column: "CartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems",
                schema: "cart");

            migrationBuilder.DropTable(
                name: "CartCustomer",
                schema: "cart");
        }
    }
}
