using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReseedTop100WithChemicalElements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clears the seeded Top100 tables so Top100DatabaseSeeder's one-time-only guard sees
            // an empty table again and reseeds from top100.seed.json (now 10 categories, with the
            // new "العناصر الكيميائية حسب العدد الذري" category added) on next app startup.
            migrationBuilder.Sql(
                """
                DELETE FROM "Top100ListItems";
                DELETE FROM "Top100Lists";
                DELETE FROM "Top100Categories";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not reversible -- the old seed content is gone, not stashed.
        }
    }
}
