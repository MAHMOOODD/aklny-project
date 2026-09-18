using Microsoft.EntityFrameworkCore.Migrations;
using Resturant_Backend.Roles;

#nullable disable

namespace Resturant_Backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[]
                {
            "Id",
            "Name",
            "NormalizedName",
            "ConcurrencyStamp"
                },
                values: new object[,]
                {
            {
                Guid.NewGuid().ToString(),
                Role.Admin,
                Role.Admin.ToUpper(),
              Guid.NewGuid().ToString()
            },
            {
               Guid.NewGuid().ToString(),
                Role.Manager,
                Role.Manager.ToUpper(),
                Guid.NewGuid().ToString()
            },
            {
              Guid.NewGuid().ToString(),
                Role.User,
                Role.User.ToUpper(),
               Guid.NewGuid().ToString()
            }
                });
        }



        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
        table: "AspNetRoles",
        keyColumn: "Name",
        keyValues: new object[]
        {
            Role.Admin,
            Role.Manager,
            Role.User
        });

        }
    }
}
