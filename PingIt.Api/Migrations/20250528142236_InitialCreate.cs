using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PingIt.Api.Migrations
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    WantsNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    Street = table.Column<string>(type: "text", nullable: false),
                    HouseNumber = table.Column<string>(type: "text", nullable: false),
                    PostalCode = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HandledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    HandledByExternal = table.Column<bool>(type: "boolean", nullable: false),
                    HandledByUserId = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Incidents_Users_HandledByUserId",
                        column: x => x.HandledByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "IncidentPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncidentId = table.Column<int>(type: "integer", nullable: false),
                    PhotoUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentPhotos_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncidentId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentStatusHistories_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncidentStatusHistories_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Incidents",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "Deadline", "Description", "HandledAt", "HandledByExternal", "HandledByUserId", "Latitude", "Longitude", "Notes", "Priority", "Status", "Title" },
                values: new object[,]
                {
                    { 8, new DateTime(2025, 6, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 7, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Na regen staat de straat voor het huis blank, vermoeden is dat deze put het probleem is", null, false, null, 51.7503m, 5.1351m, null, 3, 0, "Wateroverlast verstopte put" },
                    { 9, new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 8, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Vliegtuigen vliegen steeds lager over. Ik heb dit ooit al wel eens eerder aangegeven hier is niks mee gedaan!", null, false, null, 51.7518m, 5.1306m, null, 1, 0, "Geluidsoverlast vliegtuig" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "City", "Email", "FirstName", "HouseNumber", "LastName", "PasswordHash", "PhoneNumber", "PostalCode", "Role", "Street", "WantsNotifications" },
                values: new object[,]
                {
                    { 1, "Wijk en Aalburg", "admin@pingit.nl", "Piet", "11", "De Vries", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 2, "Dorpsstraat", false },
                    { 2, "Wijk en Aalburg", "klaas.bakker@wijkmail.nl", "Klaas", "12", "Bakker", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 2, "Dorpsstraat", true },
                    { 3, "Wijk en Aalburg", "worker@pingit.nl", "Henk", "13", "Van Dijk", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 1, "Dorpsstraat", false },
                    { 4, "Wijk en Aalburg", "gerda.smit@wijkmail.nl", "Gerda", "14", "Smit", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 1, "Dorpsstraat", true },
                    { 5, "Wijk en Aalburg", "lisa.koster@wijkmail.nl", "Lisa", "15", "Koster", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 1, "Dorpsstraat", false },
                    { 6, "Wijk en Aalburg", "thomas.de boer@wijkmail.nl", "Thomas", "16", "De Boer", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 1, "Dorpsstraat", true },
                    { 7, "Wijk en Aalburg", "sanne.vos@wijkmail.nl", "Sanne", "17", "Vos", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 0, "Dorpsstraat", false },
                    { 8, "Wijk en Aalburg", "bram.meijer@wijkmail.nl", "Bram", "18", "Meijer", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 0, "Dorpsstraat", true },
                    { 9, "Wijk en Aalburg", "eva.mulder@wijkmail.nl", "Eva", "19", "Mulder", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 0, "Dorpsstraat", false },
                    { 10, "Wijk en Aalburg", "mark.jansen@wijkmail.nl", "Mark", "20", "Jansen", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 0, "Dorpsstraat", true },
                    { 11, "Wijk en Aalburg", "inge.de vries@wijkmail.nl", "Inge", "21", "De Vries", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 0, "Dorpsstraat", false },
                    { 12, "Wijk en Aalburg", "daan.bakker@wijkmail.nl", "Daan", "22", "Bakker", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 0, "Dorpsstraat", true },
                    { 13, "Wijk en Aalburg", "julia.van dijk@wijkmail.nl", "Julia", "23", "Van Dijk", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 0, "Dorpsstraat", false },
                    { 14, "Wijk en Aalburg", "willem.smit@wijkmail.nl", "Willem", "24", "Smit", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 0, "Dorpsstraat", true },
                    { 15, "Wijk en Aalburg", "fleur.koster@wijkmail.nl", "Fleur", "25", "Koster", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 0, "Dorpsstraat", false },
                    { 16, "Wijk en Aalburg", "jan.de boer@wijkmail.nl", "Jan", "26", "De Boer", "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", "06-12345678", "4261AA", 0, "Dorpsstraat", true }
                });

            migrationBuilder.InsertData(
                table: "IncidentPhotos",
                columns: new[] { "Id", "IncidentId", "PhotoUrl" },
                values: new object[,]
                {
                    { 13, 8, "/Uploads/verstopteput1.png" },
                    { 14, 8, "/Uploads/verstopteput2.png" }
                });

            migrationBuilder.InsertData(
                table: "Incidents",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "Deadline", "Description", "HandledAt", "HandledByExternal", "HandledByUserId", "Latitude", "Longitude", "Notes", "Priority", "Status", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 6, 29, 0, 0, 0, 0, DateTimeKind.Utc), 7, new DateTime(2025, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), "De lantaarnpaal voor huisnummer 12 doet het al dagen niet meer.", null, false, 3, 51.7501m, 5.1305m, "Ingepland voor reparatie volgende week.", 2, 1, "Kapotte lantaarnpaal" },
                    { 2, new DateTime(2025, 6, 28, 0, 0, 0, 0, DateTimeKind.Utc), 8, new DateTime(2025, 8, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Overal ligt afval op het speelveld achter het winkelcentrum, echt super goor", null, false, 3, 51.7510m, 5.1310m, "Reinigingsdienst is bezig met opruimen.", 1, 2, "Zwerfvuil" },
                    { 3, new DateTime(2025, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc), 9, new DateTime(2025, 7, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Bankje in het parkje bij de kerk is denk ik vernield man. niet spang.", null, false, 3, 51.7525m, 5.1342m, "Bankje is vervangen.", 3, 3, "Kapot bankje" },
                    { 4, new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc), 10, new DateTime(2025, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Mijn moeder is bijna gevallen door een losse tegel op de hoek van de Schoolstraat.", null, false, null, 51.7499m, 5.1331m, null, 2, 0, "Losliggende stoeptegel" },
                    { 5, new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc), 11, new DateTime(2025, 7, 2, 0, 0, 0, 0, DateTimeKind.Utc), "Er ligt een grote boom over het fietspad aan de Buitenkade.", null, false, null, 51.7488m, 5.1320m, null, 4, 1, "Omgevallen boom" },
                    { 6, new DateTime(2025, 6, 29, 0, 0, 0, 0, DateTimeKind.Utc), 12, new DateTime(2025, 7, 8, 0, 0, 0, 0, DateTimeKind.Utc), "In dat steegje wordt sowieso gedealt. Constant gasten met capuchons op op scooters zijn daar aanwezig 's avonds. Ik vertrouw dit echt niet.", null, false, 3, 51.7470m, 5.1312m, "Melding doorgestuurd naar politie.", 3, 2, "Verdacht steegje mogelijke drugshandel" },
                    { 7, new DateTime(2025, 6, 28, 0, 0, 0, 0, DateTimeKind.Utc), 7, new DateTime(2025, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Auto staat al dagen op de stoep bij het gemeentehuis.", null, false, 3, 51.7490m, 5.1309m, "Voertuig is weggesleept.", 2, 3, "Foutgeparkeerd voertuig" },
                    { 10, new DateTime(2025, 6, 29, 0, 0, 0, 0, DateTimeKind.Utc), 13, new DateTime(2025, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Die jeugd heeft ook geen respect voor rust he ze zitten de hele avond harde boem boem muziek te draaien en het staat veels te hard in mijn tijd mochten we niet eens naar buiten na 10 uur want dat was gevaarlijk ik weet dat nog goed toen gingen we binnen altijd spelletjes spelen wat een mooie tijden waren dat.", null, false, null, 51.7520m, 5.1345m, null, 2, 0, "Geluidsoverlast muziek" },
                    { 11, new DateTime(2025, 6, 29, 0, 0, 0, 0, DateTimeKind.Utc), 14, new DateTime(2025, 7, 2, 0, 0, 0, 0, DateTimeKind.Utc), "Ruit van bushokje bij de Markt is vannacht ingegooid.", null, false, null, 51.7530m, 5.1327m, null, 4, 1, "Ingegooide bushalte" }
                });

            migrationBuilder.InsertData(
                table: "IncidentPhotos",
                columns: new[] { "Id", "IncidentId", "PhotoUrl" },
                values: new object[,]
                {
                    { 1, 1, "/Uploads/kapottelantaarnpaal1.png" },
                    { 2, 2, "/Uploads/zwerfvuil1.png" },
                    { 3, 2, "/Uploads/zwerfvuil2.png" },
                    { 4, 2, "/Uploads/zwerfvuil3.png" },
                    { 5, 3, "/Uploads/kapotbankje1.png" },
                    { 6, 3, "/Uploads/kapotbankje2.png" },
                    { 7, 4, "/Uploads/losliggendestoeptegel1.png" },
                    { 8, 4, "/Uploads/losliggendestoeptegel2.png" },
                    { 9, 5, "/Uploads/omgevallenboom1.png" },
                    { 10, 6, "/Uploads/steegje1.png" },
                    { 11, 7, "/Uploads/foutgeparkeerd1.png" },
                    { 12, 7, "/Uploads/foutgeparkeerd2.png" },
                    { 15, 11, "/Uploads/bushokje1.png" },
                    { 16, 11, "/Uploads/bushokje2.png" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentPhotos_IncidentId",
                table: "IncidentPhotos",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_CreatedByUserId",
                table: "Incidents",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_HandledByUserId",
                table: "Incidents",
                column: "HandledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentStatusHistories_ChangedByUserId",
                table: "IncidentStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentStatusHistories_IncidentId",
                table: "IncidentStatusHistories",
                column: "IncidentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncidentPhotos");

            migrationBuilder.DropTable(
                name: "IncidentStatusHistories");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
