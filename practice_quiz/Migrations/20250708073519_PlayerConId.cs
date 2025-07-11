using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace practice_quiz.Migrations
{
    /// <inheritdoc />
    public partial class PlayerConId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "connection_id",
                table: "player",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "connection_id",
                table: "player");
        }
    }
}
