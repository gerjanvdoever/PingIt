using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Configuration;
using PingIt.Api.Models;
using PingIt.Api.Services;
using PingIt.Shared.Enums;
using PingIt.Shared.Dtos;
using System;
using System.Collections.Generic;

namespace PingIt.Tests.Api
{
    public class JwtServiceTests
    {
        private JwtService _jwtService;

        [SetUp]
        public void SetUp()
        {
            var mockConfig = new Mock<IConfiguration>();

            mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("test-issuer");
            mockConfig.Setup(c => c["Jwt:Audience"]).Returns("test-audience");
            mockConfig.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");

            // Fake .env values
            Environment.SetEnvironmentVariable("JWT_SECRET", "supersecsdewretkey!123asdfasdfewwfhht4567890123456");

            _jwtService = new JwtService(mockConfig.Object);
        }

        [Test]
        public void GenerateToken_ValidUser_ReturnsTokenString()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "user@example.com",
                Role = Shared.Enums.UserRole.Resident,
                FirstName = "Jane",
                LastName = "Doe"
            };

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            Assert.That(token, Is.Not.Null);
            Assert.That(token.Length, Is.GreaterThan(0));
        }
    }
}
