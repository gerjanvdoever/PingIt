using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PingIt.Api.Controllers;
using PingIt.Api.Data;
using PingIt.Api.Models;
using PingIt.Shared.Dtos;
using PingIt.Shared.Enums;

namespace PingIt.Tests.Api
{
    [TestFixture]
    public class UserControllerTests
    {
        private PingItDbContext _context;
        private UserController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<PingItDbContext>()
                .UseInMemoryDatabase(databaseName: "UserTestDB_" + Guid.NewGuid())
                .Options;

            _context = new PingItDbContext(options);

            // Seed complete test data with all required fields
            _context.Users.AddRange(new List<User>
    {
        new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PasswordHash = "hashed_password_123",
            Street = "123 Main St",
            HouseNumber = "42",
            PostalCode = "1234AB",
            City = "Testville",
            Role = UserRole.Resident,
            WantsNotifications = true,
            PhoneNumber = "+1234567890"
        },
        new User
        {
            Id = 2,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            PasswordHash = "hashed_password_456",
            Street = "456 Oak Ave",
            HouseNumber = "10",
            PostalCode = "5678CD",
            City = "Test City",
            Role = UserRole.Worker,
            WantsNotifications = false,
            PhoneNumber = "+9876543210"
        },
        new User
        {
            Id = 3,
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@example.com",
            PasswordHash = "hashed_password_789",
            Street = "789 Admin Blvd",
            HouseNumber = "1",
            PostalCode = "9012EF",
            City = "Admin City",
            Role = UserRole.Administrator,
            WantsNotifications = true,
            PhoneNumber = "+1122334455"
        }
    });

            _context.SaveChanges();

            _controller = new UserController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        private void SetUserContext(int userId, UserRole role)
        {
            var claims = new List<Claim>
        {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Role, role.ToString())
        };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Test]
        public async Task GetUser_AsSelf_ReturnsUser()
        {
            // Arrange
            SetUserContext(1, UserRole.Resident);

            // Act
            var result = await _controller.GetUser(1);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var userDto = okResult.Value as UserDto;
            Assert.That(userDto.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task UpdateUser_MultipleInvalidPostalCodes_ReturnsBadRequest()
        {
            // Arrange
            SetUserContext(1, UserRole.Resident);

            var invalidPostalCodes = new[]
            {
                "Invalid",
                "12345",   
                "ABCDEF",  
                "123AB",
                "12345AB",   
                "1234ab",
            };

            foreach (var invalidPostalCode in invalidPostalCodes)
            {
                // Arrange for each iteration
                var updateDto = new UpdateUserDto
                {
                    PostalCode = invalidPostalCode
                };

                // Act
                var result = await _controller.UpdateUser(1, updateDto);

                // Assert
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>(),
                    $"Postal code '{invalidPostalCode}' should have returned BadRequest");

                var badRequestResult = result as BadRequestObjectResult;
                var response = badRequestResult?.Value?.ToString();
                Assert.That(response, Does.Contain("Postal code must be in the format 1234AB"),
                    $"Error message for '{invalidPostalCode}' should mention correct format");
            }
        }

        [Test]
        public async Task GetAllUsers_AsAdmin_ReturnsOk()
        {
            // Arrange
            SetUserContext(3, UserRole.Administrator);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var users = okResult?.Value as List<UserDto>;
            Assert.That(users, Is.Not.Null);
            Assert.That(users.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task GetAllUsers_AsNonAdmin_ReturnsForbidden()
        {
            // Arrange
            SetUserContext(1, UserRole.Resident);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
        }

        [Test]
        public async Task GetAllWorkers_ReturnsOnlyWorkers()
        {
            // Arrange
            SetUserContext(3, UserRole.Administrator);

            // Act
            var result = await _controller.GetAllWorkers();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var workers = okResult.Value as List<UserDto>;
            Assert.That(workers.Count, Is.EqualTo(1));
            Assert.That(workers[0].Id, Is.EqualTo(2));
        }

        [Test]
        public async Task UpdateUserRole_AsAdmin_UpdatesRole()
        {
            // Arrange
            SetUserContext(3, UserRole.Administrator);
            var roleDto = new UserRoleDto { Role = UserRole.Worker };

            // Act
            var result = await _controller.UpdateUserRole(1, roleDto);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());

            var updatedUser = await _context.Users.FindAsync(1);
            Assert.That(updatedUser.Role, Is.EqualTo(UserRole.Worker));
        }

        [Test]
        public async Task UpdateUserRole_AsNonAdmin_ReturnsForbidden()
        {
            // Arrange
            SetUserContext(2, UserRole.Worker);
            var roleDto = new UserRoleDto { Role = UserRole.Worker };

            // Act
            var result = await _controller.UpdateUserRole(1, roleDto);

            // Assert
            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }
    }
}