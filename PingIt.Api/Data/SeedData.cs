using Microsoft.EntityFrameworkCore;
using PingIt.Api.Models;
using PingIt.Shared.Enums;

/*
 * Seed data for the PingIt application.
 * Notable users:
 * admin@pingit.nl password: Welkom123 (Administrator)
 * worker@pingit.nl password: Welkom123 (Worker with incidents)
 * for demonstration and testing purposes, every user has password: Welkom123
 * 
 * Adds a total of 16 users (2 Admins, 4 workers, 10 residents), 11 incidents, and 18 photos (already in upload folder) to said incidents.
 */
namespace PingIt.Api.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(PingItDbContext dbContext)
        {
            if (dbContext.Users.Any()) return; // Already seeded

            var now = DateTime.UtcNow;

            var users = new List<User>();
            var random = new Random();

            string[] voornamen = { "Jan", "Piet", "Klaas", "Henk", "Gerda", "Lisa", "Thomas", "Sanne", "Bram", "Eva", "Mark", "Inge", "Daan", "Julia", "Willem", "Fleur" };
            string[] achternamen = { "Jansen", "De Vries", "Bakker", "Van Dijk", "Smit", "Koster", "De Boer", "Vos", "Meijer", "Mulder" };

            for (int i = 1; i <= 16; i++)
            {
                var voornaam = voornamen[i % voornamen.Length];
                var achternaam = achternamen[i % achternamen.Length];
                var email = $"{voornaam.ToLower()}.{achternaam.ToLower()}@wijkmail.nl";

                if (i == 1) email = "admin@pingit.nl";
                if (i == 3) email = "worker@pingit.nl";

                users.Add(new User
                {
                    FirstName = voornaam,
                    LastName = achternaam,
                    Email = email,
                    PasswordHash = "e8oCXv9PuHe+qG+qxWyQm2XJet+6I7fB+2uXctNLQg4=", // Welkom123
                    Role = i <= 2 ? UserRole.Administrator : (i <= 6 ? UserRole.Worker : UserRole.Resident),
                    PhoneNumber = $"06-12345678",
                    WantsNotifications = i % 2 == 0,
                    Street = "Dorpsstraat",
                    HouseNumber = (i + 10).ToString(),
                    PostalCode = "4261AA",
                    City = "Wijk en Aalburg"
                });
            }

            await dbContext.Users.AddRangeAsync(users);
            await dbContext.SaveChangesAsync();

            var incidents = new List<Incident>
            {
                new Incident {Title = "Kapotte lantaarnpaal", Description = "De lantaarnpaal voor huisnummer 12 doet het al dagen niet meer.", Latitude = 51.7501m, Longitude = 5.1305m, CreatedAt = now.AddDays(-2), Status = IncidentStatus.Registered, Priority = PriorityLevel.Normal, Deadline = now.AddDays(21), CreatedByUserId = 7, HandledByUserId = 3, Notes = "Ingepland voor reparatie volgende week." },
                new Incident {Title = "Zwerfvuil", Description = "Overal ligt afval op het speelveld achter het winkelcentrum, echt super goor", Latitude = 51.7510m, Longitude = 5.1310m, CreatedAt = now.AddDays(-3), Status = IncidentStatus.InProgress, Priority = PriorityLevel.Low, Deadline = now.AddDays(42), CreatedByUserId = 8, HandledByUserId = 3, Notes = "Reinigingsdienst is bezig met opruimen." },
                new Incident {Title = "Kapot bankje", Description = "Bankje in het parkje bij de kerk is denk ik vernield man. niet spang.", Latitude = 51.7525m, Longitude = 5.1342m, CreatedAt = now.AddDays(-4), Status = IncidentStatus.Resolved, Priority = PriorityLevel.High, Deadline = now.AddDays(7), CreatedByUserId = 9, HandledByUserId = 3, Notes = "Bankje is vervangen." },
                new Incident {Title = "Losliggende stoeptegel", Description = "Mijn moeder is bijna gevallen door een losse tegel op de hoek van de Schoolstraat.", Latitude = 51.7499m, Longitude = 5.1331m, CreatedAt = now.AddDays(-1), Status = IncidentStatus.Reported, CreatedByUserId = 10 },
                new Incident {Title = "Omgevallen boom", Description = "Er ligt een grote boom over het fietspad aan de Buitenkade.", Latitude = 51.7488m, Longitude = 5.1320m, CreatedAt = now.AddDays(-1), Status = IncidentStatus.Registered, Priority = PriorityLevel.Emergency, Deadline = now.AddDays(1), CreatedByUserId = 11, HandledByUserId =  4},
                new Incident {Title = "Verdacht steegje mogelijke drugshandel", Description = "In dat steegje wordt sowieso gedealt. Constant gasten met capuchons op op scooters zijn daar aanwezig 's avonds. Ik vertrouw dit echt niet.", Latitude = 51.7592m, Longitude = 5.1286m, CreatedAt = now.AddDays(-2), Status = IncidentStatus.Registered, Priority = PriorityLevel.High, Deadline = now.AddDays(7), CreatedByUserId = 12, HandledByExternal = true, Notes = "Melding doorgestuurd naar politie." },
                new Incident {Title = "Foutgeparkeerd voertuig", Description = "Auto staat al dagen op de stoep bij het gemeentehuis.", Latitude = 51.7490m, Longitude = 5.1309m, CreatedAt = now.AddDays(-3), Status = IncidentStatus.Registered, Priority = PriorityLevel.Normal, Deadline = now.AddDays(21), CreatedByUserId = 7, HandledByUserId = 3, Notes = "Voertuig moet worden weggesleept." },
                new Incident {Title = "Wateroverlast verstopte put", Description = "Na regen staat de straat voor het huis blank, vermoeden is dat deze put het probleem is", Latitude = 51.7503m, Longitude = 5.1351m, CreatedAt = now.AddDays(-5), Status = IncidentStatus.Reported, CreatedByUserId = null },
                new Incident {Title = "Geluidsoverlast vliegtuig", Description = "Vliegtuigen vliegen steeds lager over. Ik heb dit ooit al wel eens eerder aangegeven hier is niks mee gedaan!", Latitude = 51.7518m, Longitude = 5.1306m, CreatedAt = now.AddDays(-1), Status = IncidentStatus.Reported, CreatedByUserId = null },
                new Incident {Title = "Geluidsoverlast muziek", Description = "Die jeugd heeft ook geen respect voor rust he ze zitten de hele avond harde boem boem muziek te draaien en het staat veels te hard in mijn tijd mochten we niet eens naar buiten na 10 uur want dat was gevaarlijk ik weet dat nog goed toen gingen we binnen altijd spelletjes spelen wat een mooie tijden waren dat.", Latitude = 51.7520m, Longitude = 5.1345m, CreatedAt = now.AddDays(-2), Status = IncidentStatus.Reported, CreatedByUserId = 13, HandledByUserId = 3 },
                new Incident {Title = "Ingegooide bushalte", Description = "Ruit van bushokje bij de Markt is vannacht ingegooid.", Latitude = 51.7530m, Longitude = 5.1327m, CreatedAt = now.AddDays(-2), Status = IncidentStatus.Registered, Priority = PriorityLevel.Emergency, Deadline = now.AddDays(1), CreatedByUserId = 14 }
            };

            await dbContext.Incidents.AddRangeAsync(incidents);
            await dbContext.SaveChangesAsync();

            var photos = new List<IncidentPhoto>
            {
                new IncidentPhoto {IncidentId = 1, PhotoUrl = "/Uploads/kapottelantaarnpaal1.png" },
                new IncidentPhoto {IncidentId = 2, PhotoUrl = "/Uploads/zwerfvuil1.png" },
                new IncidentPhoto {IncidentId = 2, PhotoUrl = "/Uploads/zwerfvuil2.png" },
                new IncidentPhoto {IncidentId = 2, PhotoUrl = "/Uploads/zwerfvuil3.png" },
                new IncidentPhoto {IncidentId = 3, PhotoUrl = "/Uploads/kapotbankje1.png" },
                new IncidentPhoto {IncidentId = 3, PhotoUrl = "/Uploads/kapotbankje2.png" },
                new IncidentPhoto {IncidentId = 4, PhotoUrl = "/Uploads/losliggendestoeptegel1.png" },
                new IncidentPhoto {IncidentId = 4, PhotoUrl = "/Uploads/losliggendestoeptegel2.png" },
                new IncidentPhoto {IncidentId = 5, PhotoUrl = "/Uploads/omgevallenboom1.png" },
                new IncidentPhoto {IncidentId = 6, PhotoUrl = "/Uploads/steegje1.png" },
                new IncidentPhoto {IncidentId = 7, PhotoUrl = "/Uploads/foutgeparkeerd1.png" },
                new IncidentPhoto {IncidentId = 7, PhotoUrl = "/Uploads/foutgeparkeerd2.png" },
                new IncidentPhoto {IncidentId = 8, PhotoUrl = "/Uploads/verstopteput1.png" },
                new IncidentPhoto {IncidentId = 8, PhotoUrl = "/Uploads/verstopteput2.png" },
                new IncidentPhoto {IncidentId = 9, PhotoUrl = "/Uploads/geluidsoverlastvliegtuig1.png" },
                new IncidentPhoto {IncidentId = 9, PhotoUrl = "/Uploads/geluidsoverlastvliegtuig2.png" },
                new IncidentPhoto {IncidentId = 11, PhotoUrl = "/Uploads/bushokje1.png" },
                new IncidentPhoto {IncidentId = 11, PhotoUrl = "/Uploads/bushokje2.png" }
            };

            await dbContext.IncidentPhotos.AddRangeAsync(photos);
            await dbContext.SaveChangesAsync();
        }
    }
}
