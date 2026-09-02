using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Top100GameSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "RankingGameSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "PasswordGameSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "GameSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "Categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Top100GameSessions_UserId",
                table: "Top100GameSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RankingGameSessions_UserId",
                table: "RankingGameSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordGameSessions_UserId",
                table: "PasswordGameSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_UserId",
                table: "GameSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_OwnerUserId",
                table: "Categories",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_OwnerUserId",
                table: "Categories",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_Users_UserId",
                table: "GameSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordGameSessions_Users_UserId",
                table: "PasswordGameSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RankingGameSessions_Users_UserId",
                table: "RankingGameSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Top100GameSessions_Users_UserId",
                table: "Top100GameSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_OwnerUserId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_Users_UserId",
                table: "GameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_PasswordGameSessions_Users_UserId",
                table: "PasswordGameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_RankingGameSessions_Users_UserId",
                table: "RankingGameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Top100GameSessions_Users_UserId",
                table: "Top100GameSessions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Top100GameSessions_UserId",
                table: "Top100GameSessions");

            migrationBuilder.DropIndex(
                name: "IX_RankingGameSessions_UserId",
                table: "RankingGameSessions");

            migrationBuilder.DropIndex(
                name: "IX_PasswordGameSessions_UserId",
                table: "PasswordGameSessions");

            migrationBuilder.DropIndex(
                name: "IX_GameSessions_UserId",
                table: "GameSessions");

            migrationBuilder.DropIndex(
                name: "IX_Categories_OwnerUserId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Top100GameSessions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RankingGameSessions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PasswordGameSessions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Categories");
        }
    }
}
