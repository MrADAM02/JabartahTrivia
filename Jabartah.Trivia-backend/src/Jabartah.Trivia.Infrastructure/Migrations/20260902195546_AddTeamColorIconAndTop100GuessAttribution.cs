using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamColorIconAndTop100GuessAttribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Top100Teams",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Top100Teams",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid[]>(
                name: "GuessedByTeamIds",
                table: "Top100Rounds",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Teams",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Teams",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "RankingTeams",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "RankingTeams",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "PasswordTeams",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "PasswordTeams",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Top100Teams");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Top100Teams");

            migrationBuilder.DropColumn(
                name: "GuessedByTeamIds",
                table: "Top100Rounds");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "RankingTeams");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "RankingTeams");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "PasswordTeams");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "PasswordTeams");
        }
    }
}
