using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAttemptFailed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttemptFailed",
                table: "GameQuestionStates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AttemptFailed",
                table: "GameQuestionStates",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
