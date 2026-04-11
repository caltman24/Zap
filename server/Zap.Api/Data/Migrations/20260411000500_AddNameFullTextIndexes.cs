using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zap.Api.Data.Migrations;

/// <inheritdoc />
public partial class AddNameFullTextIndexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE INDEX IF NOT EXISTS "IX_Tickets_Name_FullText"
            ON "Tickets"
            USING GIN (to_tsvector('simple', "Name"));
            """);

        migrationBuilder.Sql(
            """
            CREATE INDEX IF NOT EXISTS "IX_Projects_Name_FullText"
            ON "Projects"
            USING GIN (to_tsvector('simple', "Name"));
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP INDEX IF EXISTS "IX_Tickets_Name_FullText";
            """);
        migrationBuilder.Sql("""
            DROP INDEX IF EXISTS "IX_Projects_Name_FullText";
            """);
    }
}
