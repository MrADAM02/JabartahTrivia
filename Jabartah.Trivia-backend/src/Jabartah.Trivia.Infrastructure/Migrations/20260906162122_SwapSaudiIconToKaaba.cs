using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SwapSaudiIconToKaaba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Swapped السعودية's icon from 🕌 (mosque, generic-Islam) to 🕋 (Kaaba) --
            // unambiguously Saudi-specific and, unlike the flag emoji tried earlier,
            // a real standalone pictograph that renders correctly on Windows too.
            migrationBuilder.Sql(
                """
                DELETE FROM "RankingListItems";
                DELETE FROM "RankingLists";
                DELETE FROM "RankingCategories";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not reversible -- the old seed content is gone, not stashed.
        }
    }
}
