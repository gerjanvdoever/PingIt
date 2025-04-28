using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PingIt.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHandledFieldsToIncident : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HandledByExternal",
                table: "Incidents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "HandledByUserId",
                table: "Incidents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Incidents",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_HandledByUserId",
                table: "Incidents",
                column: "HandledByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Users_HandledByUserId",
                table: "Incidents",
                column: "HandledByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Users_HandledByUserId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_HandledByUserId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "HandledByExternal",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "HandledByUserId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Incidents");
        }
    }
}
