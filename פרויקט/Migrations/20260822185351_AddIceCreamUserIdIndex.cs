using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMiddleware.Migrations
{
    // The UserId index improves JOIN/lookup operations from an O(n) full scan to O(log n) B-tree lookups.
    // With the current small dataset the impact is negligible, but it pays off as users and ice creams grow,
    // especially because the stats and ranking endpoints execute JOINs.
    /// <inheritdoc />
    public partial class AddIceCreamUserIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_IceCreams_UserId",
                table: "IceCreams",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IceCreams_UserId",
                table: "IceCreams");
        }
    }
}
