using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PingIt.Api.Controllers;
using PingIt.Api.Data;
using PingIt.Api.Models;
using PingIt.Api.Services;
using PingIt.Api.Services.PingIt.Api.Services;
using PingIt.Shared.Dtos;
using PingIt.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PingIt.Tests.Api
{
    public class IncidentStatusControllerTests
    {
        private PingItDbContext _context;
        private Mock<IEmailService> _mockEmailService;
        private IncidentStatusController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<PingItDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDB_" + Guid.NewGuid())
                .Options;
            _context = new PingItDbContext(options);

            // Seed complete user data
            _context.Users.Add(new User
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PasswordHash = "hashedpw",
                Street = "123 Main St",
                HouseNumber = "1",
                City = "Testville",
                PostalCode = "12345",
                WantsNotifications = true
            });

            _context.Incidents.Add(new Incident
            {
                Id = 1,
                Title = "Broken Streetlight",
                Status = IncidentStatus.Reported,
                CreatedByUserId = 1
            });

            _context.SaveChanges();

            _mockEmailService = new Mock<IEmailService>();
            _controller = new IncidentStatusController(_context, _mockEmailService.Object);

            // Mock HttpContext
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Worker")
            }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        [Test]
        public async Task ChangeStatus_ValidRequest_UpdatesIncidentAndReturnsOk()
        {
            // Arrange
            var dto = new IncidentStatusUpdateDto
            {
                NewStatus = IncidentStatus.InProgress,
                Notes = "Worker on the way"
            };

            // Act
            var result = await _controller.ChangeStatus(1, dto);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult?.Value?.ToString(), Does.Contain("Incident updated successfully"));
        }

        [Test]
        public async Task ChangeStatus_UserWantsNotifications_SendsEmail()
        {
            // Arrange
            var dto = new IncidentStatusUpdateDto
            {
                NewStatus = IncidentStatus.InProgress,
                Notes = "Worker on the way"
            };

            // Act
            await _controller.ChangeStatus(1, dto);

            // Assert
            _mockEmailService.Verify(
                x => x.SendEmailAsync(
                    "test@example.com",
                    "Update on your incident: Broken Streetlight",
                    It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public async Task ChangeStatus_UserDoesNotWantNotifications_DoesNotSendEmail()
        {
            // Arrange
            var userWithoutNotifications = new User
            {
                Id = 2,
                FirstName = "No",
                LastName = "Notifications",
                Email = "no@example.com",
                PasswordHash = "hashedpw",
                Street = "456 Other St",
                HouseNumber = "2",
                City = "Testville",
                PostalCode = "12345",
                WantsNotifications = false // main diff
            };

            var incidentWithoutNotifications = new Incident
            {
                Id = 2,
                Title = "Another Issue",
                Status = IncidentStatus.Reported,
                CreatedByUserId = 2
            };

            _context.Users.Add(userWithoutNotifications);
            _context.Incidents.Add(incidentWithoutNotifications);
            await _context.SaveChangesAsync();

            var dto = new IncidentStatusUpdateDto
            {
                NewStatus = IncidentStatus.InProgress,
                Notes = "Worker on the way"
            };

            // Act
            await _controller.ChangeStatus(2, dto);

            // Assert - this time if it was not sent
            _mockEmailService.Verify(
                x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
    }
}