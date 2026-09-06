using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandTriviaCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Scoped delete, not a blanket wipe -- Trivia (unlike Password/Ranking/Top100)
            // has real user-owned "تصنيفاتي" categories (OwnerUserId IS NOT NULL) that must
            // survive this reseed. Only the seeded/global categories (OwnerUserId IS NULL)
            // and their questions are cleared so DatabaseSeeder's one-time-only guard
            // re-seeds them from the updated categories.seed.json on next startup.
            migrationBuilder.Sql(
                "DELETE FROM \"Questions\" WHERE \"CategoryId\" IN (SELECT \"Id\" FROM \"Categories\" WHERE \"OwnerUserId\" IS NULL);");
            migrationBuilder.Sql(
                "DELETE FROM \"Categories\" WHERE \"OwnerUserId\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
