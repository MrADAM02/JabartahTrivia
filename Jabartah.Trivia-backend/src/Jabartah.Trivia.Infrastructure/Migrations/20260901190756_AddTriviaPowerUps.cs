using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTriviaPowerUps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DoublePointsAvailable",
                table: "Teams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TwoAnswersAvailable",
                table: "Teams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ActivePowerUp",
                table: "GameQuestionStates",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AttemptFailed",
                table: "GameQuestionStates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsResolved",
                table: "GameQuestionStates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PowerUpTeamId",
                table: "GameQuestionStates",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoublePointsAvailable",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "TwoAnswersAvailable",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "ActivePowerUp",
                table: "GameQuestionStates");

            migrationBuilder.DropColumn(
                name: "AttemptFailed",
                table: "GameQuestionStates");

            migrationBuilder.DropColumn(
                name: "IsResolved",
                table: "GameQuestionStates");

            migrationBuilder.DropColumn(
                name: "PowerUpTeamId",
                table: "GameQuestionStates");
        }
    }
}
