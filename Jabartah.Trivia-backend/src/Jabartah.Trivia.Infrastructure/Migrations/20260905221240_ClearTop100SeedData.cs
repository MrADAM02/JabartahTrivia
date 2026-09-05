using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ClearTop100SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Wipes the old thin/off-brand categories (18-22 items each) so Top100DatabaseSeeder's
            // one-time-only seed guard (`if (await db.Top100Categories.AnyAsync()) return;`) sees an
            // empty table again and reseeds from the replaced top100.seed.json on next app startup.
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
            // Not reversible -- the old seed content is gone, not stashed. Re-running the seeder
            // (after this migration and a fresh top100.seed.json) is what repopulates the tables.
        }
    }
}
