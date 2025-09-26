using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.CloudGames.Customer.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ZipCode",
                schema: "customer",
                table: "Addresses",
                newName: "PostalCode");

            migrationBuilder.RenameColumn(
                name: "Complement",
                schema: "customer",
                table: "Addresses",
                newName: "AdditionalInfo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PostalCode",
                schema: "customer",
                table: "Addresses",
                newName: "ZipCode");

            migrationBuilder.RenameColumn(
                name: "AdditionalInfo",
                schema: "customer",
                table: "Addresses",
                newName: "Complement");
        }
    }
}
