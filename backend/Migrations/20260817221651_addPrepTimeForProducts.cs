using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resturant_Backend.Migrations
{
    /// <inheritdoc />
    public partial class addPrepTimeForProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreparingTime",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreparingTime",
                table: "Products");
        }
    }
}
