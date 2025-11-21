using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NOX_Backend.Migrations
{
    /// <inheritdoc />
    public partial class Addnewtaskprogress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration exists only to keep EF Core migration history consistent.
            // No schema changes are applied here intentionally.
            migrationBuilder.Sql("-- No changes. Safe placeholder migration.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No rollback needed because no changes were made in Up().
            migrationBuilder.Sql("-- No rollback. Placeholder migration.");
        }
    }
}
