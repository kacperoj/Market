using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedByAi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GeneratedByAi",
                table: "Auctions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneratedByAi",
                table: "Auctions");
        }
    }
}
