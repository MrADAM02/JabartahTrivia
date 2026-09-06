using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jabartah.Trivia.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandPasswordCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Blanket wipe, not scoped -- unlike Trivia's Categories, PasswordCategory has
            // no OwnerUserId/custom-category concept, so there's no user data to preserve here.
            migrationBuilder.Sql("DELETE FROM \"PasswordWords\";");
            migrationBuilder.Sql("DELETE FROM \"PasswordCategories\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
