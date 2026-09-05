using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ClearTop100SeedDataFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The prior ClearTop100SeedData migration ran before top100.seed.json was actually
            // replaced with the new 9-category content, so the seeder immediately refilled the
            // tables with the old data on that same startup. Clearing again now that the real
            // replacement content is in place so the next startup reseeds for real.
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
