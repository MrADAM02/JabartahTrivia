using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordAndRankingGameModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordGameSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CategoryIds = table.Column<Guid[]>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordGameSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordRevealTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PasswordRoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordWordId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordRevealTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordWords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Word = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordWords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RankingCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RankingGameSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CategoryIds = table.Column<Guid[]>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingGameSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RankingListItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RankingListId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CorrectPosition = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingListItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RankingLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RankingCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordRounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordGameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoundNumber = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordWordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordRounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordRounds_PasswordGameSessions_PasswordGameSessionId",
                        column: x => x.PasswordGameSessionId,
                        principalTable: "PasswordGameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordTeams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordGameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    TurnOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordTeams_PasswordGameSessions_PasswordGameSessionId",
                        column: x => x.PasswordGameSessionId,
                        principalTable: "PasswordGameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RankingRounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RankingGameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoundNumber = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    RankingListId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PointsAwarded = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingRounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RankingRounds_RankingGameSessions_RankingGameSessionId",
                        column: x => x.RankingGameSessionId,
                        principalTable: "RankingGameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RankingTeams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RankingGameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    TurnOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RankingTeams_RankingGameSessions_RankingGameSessionId",
                        column: x => x.RankingGameSessionId,
                        principalTable: "RankingGameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordRevealTokens_PasswordRoundId",
                table: "PasswordRevealTokens",
                column: "PasswordRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordRevealTokens_Token",
                table: "PasswordRevealTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordRounds_PasswordGameSessionId_RoundNumber",
                table: "PasswordRounds",
                columns: new[] { "PasswordGameSessionId", "RoundNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordTeams_PasswordGameSessionId",
                table: "PasswordTeams",
                column: "PasswordGameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordWords_PasswordCategoryId",
                table: "PasswordWords",
                column: "PasswordCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RankingListItems_RankingListId",
                table: "RankingListItems",
                column: "RankingListId");

            migrationBuilder.CreateIndex(
                name: "IX_RankingListItems_RankingListId_CorrectPosition",
                table: "RankingListItems",
                columns: new[] { "RankingListId", "CorrectPosition" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RankingLists_RankingCategoryId",
                table: "RankingLists",
                column: "RankingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RankingRounds_RankingGameSessionId_RoundNumber",
                table: "RankingRounds",
                columns: new[] { "RankingGameSessionId", "RoundNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RankingTeams_RankingGameSessionId",
                table: "RankingTeams",
                column: "RankingGameSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordCategories");

            migrationBuilder.DropTable(
                name: "PasswordRevealTokens");

            migrationBuilder.DropTable(
                name: "PasswordRounds");

            migrationBuilder.DropTable(
                name: "PasswordTeams");

            migrationBuilder.DropTable(
                name: "PasswordWords");

            migrationBuilder.DropTable(
                name: "RankingCategories");

            migrationBuilder.DropTable(
                name: "RankingListItems");

            migrationBuilder.DropTable(
                name: "RankingLists");

            migrationBuilder.DropTable(
                name: "RankingRounds");

            migrationBuilder.DropTable(
                name: "RankingTeams");

            migrationBuilder.DropTable(
                name: "PasswordGameSessions");

            migrationBuilder.DropTable(
                name: "RankingGameSessions");
        }
    }
}
