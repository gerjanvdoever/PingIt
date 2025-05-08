using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PingIt.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Incidents",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "Deadline", "Description", "HandledAt", "HandledByExternal", "HandledByUserId", "Latitude", "Longitude", "Notes", "Priority", "Status", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 5, 8, 9, 20, 47, 1, DateTimeKind.Utc).AddTicks(7498), 2, null, "Werkt niet sinds gisteren", null, false, 1, 52.3702m, 4.8952m, null, 2, 0, "Kapotte lantaarnpaal" },
                    { 2, new DateTime(2025, 5, 8, 9, 20, 47, 1, DateTimeKind.Utc).AddTicks(9026), 2, null, "Losliggende stoeptegel bij de speeltuin", null, false, 1, 52.3792m, 4.8922m, null, 3, 2, "Gevaarlijke stoeptegel" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Incidents",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Incidents",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
