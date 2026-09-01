using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTop100GameMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Top100Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Top100Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Top100GameSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RoundsPerTeam = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CategoryIds = table.Column<Guid[]>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Top100GameSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Top100ListItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Top100ListId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    AlternateSpellings = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Top100ListItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Top100Lists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Top100CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Top100Lists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Top100Rounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Top100GameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoundNumber = table.Column<int>(type: "integer", nullable: false),
                    Top100ListId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CurrentTurnTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuessesMade = table.Column<int>(type: "integer", nullable: false),
                    MaxGuesses = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GuessedItemIds = table.Column<Guid[]>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Top100Rounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Top100Rounds_Top100GameSessions_Top100GameSessionId",
                        column: x => x.Top100GameSessionId,
                        principalTable: "Top100GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Top100Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Top100GameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    TurnOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Top100Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Top100Teams_Top100GameSessions_Top100GameSessionId",
                        column: x => x.Top100GameSessionId,
                        principalTable: "Top100GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Top100ListItems_Top100ListId",
                table: "Top100ListItems",
                column: "Top100ListId");

            migrationBuilder.CreateIndex(
                name: "IX_Top100ListItems_Top100ListId_Position",
                table: "Top100ListItems",
                columns: new[] { "Top100ListId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Top100Lists_Top100CategoryId",
                table: "Top100Lists",
                column: "Top100CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Top100Rounds_Top100GameSessionId_RoundNumber",
                table: "Top100Rounds",
                columns: new[] { "Top100GameSessionId", "RoundNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Top100Teams_Top100GameSessionId",
                table: "Top100Teams",
                column: "Top100GameSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Top100Categories");

            migrationBuilder.DropTable(
                name: "Top100ListItems");

            migrationBuilder.DropTable(
                name: "Top100Lists");

            migrationBuilder.DropTable(
                name: "Top100Rounds");

            migrationBuilder.DropTable(
                name: "Top100Teams");

            migrationBuilder.DropTable(
                name: "Top100GameSessions");
        }
    }
}
