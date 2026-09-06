using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSaudiCategoryIcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // السعودية's icon was the 🇸🇦 flag emoji, which Windows renders as literal "SA"
            // text instead of a flag graphic (no OS-level regional-indicator flag support).
            // Swapped to 🕌 in ranking.seed.json -- reseed to pick it up.
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
