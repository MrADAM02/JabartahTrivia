using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondRoundRankingCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clears the seeded Ranking tables so RankingDatabaseSeeder's one-time-only guard
            // sees an empty table again and reseeds from ranking.seed.json (now 18 categories,
            // 90 lists, with 5 new categories added: موسيقى، جسم الإنسان، معالم ومبانٍ،
            // مواصلات ونقل، السعودية) on next app startup.
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
