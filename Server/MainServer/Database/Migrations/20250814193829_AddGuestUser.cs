using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.SFU.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "guest-role-id", null, "guest", "GUEST" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "guest-role-id");
        }
    }
}
