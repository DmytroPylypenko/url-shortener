using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UrlShortener.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AboutContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AboutContents_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UrlRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    ShortCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastAccessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VisitCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrlRecords_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, "admin@example.com", "Admin", "cWk1XfCdVYjhua1t1cWObQ==:O13bIcnMg4zUyFhyF1PYgXWnWRPMbvpIjjWm5P/W5UA=", "Admin" },
                    { 2, "user@example.com", "TestUser", "R3oPf4sQD09/R5n2/0w3tg==:YCLcqto/32a/pXqtAI9E2y/Wu62xu7gJewrkl6f5RvA=", "User" }
                });

            migrationBuilder.InsertData(
                table: "AboutContents",
                columns: new[] { "Id", "Content", "LastUpdatedAtUtc", "UpdatedByUserId" },
                values: new object[] { 1, "Initial About Page Content...", new DateTime(2025, 12, 1, 12, 52, 6, 144, DateTimeKind.Utc).AddTicks(3051), 1 });

            migrationBuilder.CreateIndex(
                name: "IX_AboutContents_UpdatedByUserId",
                table: "AboutContents",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UrlRecords_CreatedByUserId",
                table: "UrlRecords",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UrlRecords_OriginalUrl",
                table: "UrlRecords",
                column: "OriginalUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrlRecords_ShortCode",
                table: "UrlRecords",
                column: "ShortCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AboutContents");

            migrationBuilder.DropTable(
                name: "UrlRecords");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
