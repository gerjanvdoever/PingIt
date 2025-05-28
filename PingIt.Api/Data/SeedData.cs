using Microsoft.EntityFrameworkCore;
using PingIt.Api.Models;
using PingIt.Shared.Enums;

namespace PingIt.Api.Data
{
    public static class SeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            var staticNow = new DateTime(2025, 07, 01, 0, 0, 0, DateTimeKind.Utc);

            var users = new List<User>();
            var random = new Random();

            string[] voornamen = { "Jan", "Piet", "Klaas", "Henk", "Gerda", "Lisa", "Thomas", "Sanne", "Bram", "Eva", "Mark", "Inge", "Daan", "Julia", "Willem", "Fleur" };
            string[] achternamen = { "Jansen", "De Vries", "Bakker", "Van Dijk", "Smit", "Koster", "De Boer", "Vos", "Meijer", "Mulder" };

            for (int i = 1; i <= 16; i++)
            {
                var voornaam = voornamen[i % voornamen.Length];
                var achternaam = achternamen[i % achternamen.Length];
                var email = $"{voornaam.ToLower()}.{achternaam.ToLower()}@wijkmail.nl";

                if (i == 1) email = "admin@pingit.nl"; // Administrator
                if (i == 3) email = "worker@pingit.nl"; // Worker with incidents

                users.Add(new User
                {
                    Id = i,
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

            modelBuilder.Entity<User>().HasData(users);

            var incidents = new List<Incident>
            {
                new Incident { Id = 1, Title = "Kapotte lantaarnpaal", Description = "De lantaarnpaal voor huisnummer 12 doet het al dagen niet meer.", Latitude = 51.7501m, Longitude = 5.1305m, CreatedAt = staticNow.AddDays(-2), Status = IncidentStatus.Registered, Priority = PriorityLevel.Normal, Deadline = staticNow.AddDays(21), CreatedByUserId = 7, HandledByUserId = 3, Notes = "Ingepland voor reparatie volgende week." },
                new Incident { Id = 2, Title = "Zwerfvuil", Description = "Overal ligt afval op het speelveld achter het winkelcentrum, echt super goor", Latitude = 51.7510m, Longitude = 5.1310m, CreatedAt = staticNow.AddDays(-3), Status = IncidentStatus.InProgress, Priority = PriorityLevel.Low, Deadline = staticNow.AddDays(42), CreatedByUserId = 8, HandledByUserId = 3, Notes = "Reinigingsdienst is bezig met opruimen." },
                new Incident { Id = 3, Title = "Kapot bankje", Description = "Bankje in het parkje bij de kerk is denk ik vernield man. niet spang.", Latitude = 51.7525m, Longitude = 5.1342m, CreatedAt = staticNow.AddDays(-4), Status = IncidentStatus.Resolved, Priority = PriorityLevel.High, Deadline = staticNow.AddDays(7), CreatedByUserId = 9, HandledByUserId = 3, Notes = "Bankje is vervangen." },
                new Incident { Id = 4, Title = "Losliggende stoeptegel", Description = "Mijn moeder is bijna gevallen door een losse tegel op de hoek van de Schoolstraat.", Latitude = 51.7499m, Longitude = 5.1331m, CreatedAt = staticNow.AddDays(-1), Status = IncidentStatus.Reported, Priority = PriorityLevel.Normal, Deadline = staticNow.AddDays(21), CreatedByUserId = 10 },
                new Incident { Id = 5, Title = "Omgevallen boom", Description = "Er ligt een grote boom over het fietspad aan de Buitenkade.", Latitude = 51.7488m, Longitude = 5.1320m, CreatedAt = staticNow.AddDays(-1), Status = IncidentStatus.Registered, Priority = PriorityLevel.Emergency, Deadline = staticNow.AddDays(1), CreatedByUserId = 11 },
                new Incident { Id = 6, Title = "Verdacht steegje mogelijke drugshandel", Description = "In dat steegje wordt sowieso gedealt. Constant gasten met capuchons op op scooters zijn daar aanwezig 's avonds. Ik vertrouw dit echt niet.", Latitude = 51.7470m, Longitude = 5.1312m, CreatedAt = staticNow.AddDays(-2), Status = IncidentStatus.InProgress, Priority = PriorityLevel.High, Deadline = staticNow.AddDays(7), CreatedByUserId = 12, HandledByUserId = 3, Notes = "Melding doorgestuurd naar politie." },
                new Incident { Id = 7, Title = "Foutgeparkeerd voertuig", Description = "Auto staat al dagen op de stoep bij het gemeentehuis.", Latitude = 51.7490m, Longitude = 5.1309m, CreatedAt = staticNow.AddDays(-3), Status = IncidentStatus.Resolved, Priority = PriorityLevel.Normal, Deadline = staticNow.AddDays(21), CreatedByUserId = 7, HandledByUserId = 3, Notes = "Voertuig is weggesleept." },
                new Incident { Id = 8, Title = "Wateroverlast verstopte put", Description = "Na regen staat de straat voor het huis blank, vermoeden is dat deze put het probleem is", Latitude = 51.7503m, Longitude = 5.1351m, CreatedAt = staticNow.AddDays(-5), Status = IncidentStatus.Reported, Priority = PriorityLevel.High, Deadline = staticNow.AddDays(7), CreatedByUserId = null },
                new Incident { Id = 9, Title = "Geluidsoverlast vliegtuig", Description = "Vliegtuigen vliegen steeds lager over. Ik heb dit ooit al wel eens eerder aangegeven hier is niks mee gedaan!", Latitude = 51.7518m, Longitude = 5.1306m, CreatedAt = staticNow.AddDays(-1), Status = IncidentStatus.Reported, Priority = PriorityLevel.Low, Deadline = staticNow.AddDays(42), CreatedByUserId = null },
                new Incident { Id = 10, Title = "Geluidsoverlast muziek", Description = "Die jeugd heeft ook geen respect voor rust he ze zitten de hele avond harde boem boem muziek te draaien en het staat veels te hard in mijn tijd mochten we niet eens naar buiten na 10 uur want dat was gevaarlijk ik weet dat nog goed toen gingen we binnen altijd spelletjes spelen wat een mooie tijden waren dat.", Latitude = 51.7520m, Longitude = 5.1345m, CreatedAt = staticNow.AddDays(-2), Status = IncidentStatus.Reported, Priority = PriorityLevel.Normal, Deadline = staticNow.AddDays(21), CreatedByUserId = 13 },
                new Incident { Id = 11, Title = "Ingegooide bushalte", Description = "Ruit van bushokje bij de Markt is vannacht ingegooid.", Latitude = 51.7530m, Longitude = 5.1327m, CreatedAt = staticNow.AddDays(-2), Status = IncidentStatus.Registered, Priority = PriorityLevel.Emergency, Deadline = staticNow.AddDays(1), CreatedByUserId = 14 }
            };

            modelBuilder.Entity<Incident>().HasData(incidents);

            var photos = new List<IncidentPhoto>
            {
                new IncidentPhoto { Id = 1, IncidentId = 1, PhotoUrl = "/Uploads/kapottelantaarnpaal1.png" },
                new IncidentPhoto { Id = 2, IncidentId = 2, PhotoUrl = "/Uploads/zwerfvuil1.png" },
                new IncidentPhoto { Id = 3, IncidentId = 2, PhotoUrl = "/Uploads/zwerfvuil2.png" },
                new IncidentPhoto { Id = 4, IncidentId = 2, PhotoUrl = "/Uploads/zwerfvuil3.png" },
                new IncidentPhoto { Id = 5, IncidentId = 3, PhotoUrl = "/Uploads/kapotbankje1.png" },
                new IncidentPhoto { Id = 6, IncidentId = 3, PhotoUrl = "/Uploads/kapotbankje2.png" },
                new IncidentPhoto { Id = 7, IncidentId = 4, PhotoUrl = "/Uploads/losliggendestoeptegel1.png" },
                new IncidentPhoto { Id = 8, IncidentId = 4, PhotoUrl = "/Uploads/losliggendestoeptegel2.png" },
                new IncidentPhoto { Id = 9, IncidentId = 5, PhotoUrl = "/Uploads/omgevallenboom1.png" },
                new IncidentPhoto { Id = 10, IncidentId = 6, PhotoUrl = "/Uploads/steegje1.png" },
                new IncidentPhoto { Id = 11, IncidentId = 7, PhotoUrl = "/Uploads/foutgeparkeerd1.png" },
                new IncidentPhoto { Id = 12, IncidentId = 7, PhotoUrl = "/Uploads/foutgeparkeerd2.png" },
                new IncidentPhoto { Id = 13, IncidentId = 8, PhotoUrl = "/Uploads/verstopteput1.png" },
                new IncidentPhoto { Id = 14, IncidentId = 8, PhotoUrl = "/Uploads/verstopteput2.png" },
                new IncidentPhoto { Id = 15, IncidentId = 11, PhotoUrl = "/Uploads/bushokje1.png" },
                new IncidentPhoto { Id = 16, IncidentId = 11, PhotoUrl = "/Uploads/bushokje2.png" }
            };

            modelBuilder.Entity<IncidentPhoto>().HasData(photos);
        }
    }
}
