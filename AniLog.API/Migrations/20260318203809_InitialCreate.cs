using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AniLog.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnimeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MalId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    TitleJapanese = table.Column<string>(type: "text", nullable: true),
                    Genres = table.Column<string>(type: "text", nullable: true),
                    Episodes = table.Column<int>(type: "integer", nullable: true),
                    MalScore = table.Column<decimal>(type: "numeric", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    MyScore = table.Column<decimal>(type: "numeric", nullable: false),
                    MyStatus = table.Column<string>(type: "text", nullable: false),
                    EpisodesWatched = table.Column<int>(type: "integer", nullable: false),
                    MyNotes = table.Column<string>(type: "text", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimeLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnimeLogs_MalId",
                table: "AnimeLogs",
                column: "MalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnimeLogs");
        }
    }
}
