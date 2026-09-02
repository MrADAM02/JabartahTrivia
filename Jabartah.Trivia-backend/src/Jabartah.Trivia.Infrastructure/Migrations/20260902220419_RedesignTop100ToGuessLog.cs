using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RedesignTop100ToGuessLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Top100Rounds_Top100GameSessionId_RoundNumber",
                table: "Top100Rounds");

            migrationBuilder.DropColumn(
                name: "GuessedByTeamIds",
                table: "Top100Rounds");

            migrationBuilder.DropColumn(
                name: "GuessedItemIds",
                table: "Top100Rounds");

            migrationBuilder.DropColumn(
                name: "RoundNumber",
                table: "Top100Rounds");

            migrationBuilder.RenameColumn(
                name: "RoundsPerTeam",
                table: "Top100GameSessions",
                newName: "GuessesPerTeam");

            migrationBuilder.CreateTable(
                name: "Top100Guesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Top100RoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuessText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MatchedItemId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Top100Guesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Top100Guesses_Top100Rounds_Top100RoundId",
                        column: x => x.Top100RoundId,
                        principalTable: "Top100Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Top100Rounds_Top100GameSessionId",
                table: "Top100Rounds",
                column: "Top100GameSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Top100Guesses_Top100RoundId_SequenceNumber",
                table: "Top100Guesses",
                columns: new[] { "Top100RoundId", "SequenceNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Top100Guesses");

            migrationBuilder.DropIndex(
                name: "IX_Top100Rounds_Top100GameSessionId",
                table: "Top100Rounds");

            migrationBuilder.RenameColumn(
                name: "GuessesPerTeam",
                table: "Top100GameSessions",
                newName: "RoundsPerTeam");

            migrationBuilder.AddColumn<Guid[]>(
                name: "GuessedByTeamIds",
                table: "Top100Rounds",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);

            migrationBuilder.AddColumn<Guid[]>(
                name: "GuessedItemIds",
                table: "Top100Rounds",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);

            migrationBuilder.AddColumn<int>(
                name: "RoundNumber",
                table: "Top100Rounds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Top100Rounds_Top100GameSessionId_RoundNumber",
                table: "Top100Rounds",
                columns: new[] { "Top100GameSessionId", "RoundNumber" },
                unique: true);
        }
    }
}
