using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using PingIt.Maui.ViewModels;
using PingIt.Maui.Services;
using PingIt.Shared.Dtos;
using PingIt.Shared.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace PingIt.Tests.Maui
{
    [TestFixture]
    public class AccountViewModelTests
    {
        private Mock<ITokenStorageService> _mockTokenStorage;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<ILogger<AccountViewModel>> _mockLogger;
        private Mock<IIncidentStore> _mockIncidentStore;
        private Mock<IUserStore> _mockUserStore;
        private AccountViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            _mockTokenStorage = new Mock<ITokenStorageService>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<AccountViewModel>>();
            _mockIncidentStore = new Mock<IIncidentStore>();
            _mockUserStore = new Mock<IUserStore>();

            _viewModel = new AccountViewModel(
                _mockTokenStorage.Object,
                _mockHttpClientFactory.Object,
                _mockLogger.Object,
                _mockIncidentStore.Object,
                _mockUserStore.Object);
        }

        [Test]
        public async Task LoadIncidentsAsync_SuccessfulResponse_UpdatesIncidentsAndActiveCount()
        {
            // Arrange
            var userId = 12;
            _mockTokenStorage.Setup(x => x.UserId).Returns(userId);

            var testIncidents = new IncidentDto[]
            {
        new() {
            Id = 1,
            Title = "Test 1",
            Status = IncidentStatus.Reported,
            CreatedAt = DateTime.UtcNow,
            Latitude = 1.0m,
            Longitude = 1.0m
        },
        new() {
            Id = 2,
            Title = "Test 2",
            Status = IncidentStatus.Registered,
            CreatedAt = DateTime.UtcNow,
            Latitude = 2.0m,
            Longitude = 2.0m
        },
        new() {
            Id = 3,
            Title = "Test 3",
            Status = IncidentStatus.Resolved,
            CreatedAt = DateTime.UtcNow,
            Latitude = 3.0m,
            Longitude = 3.0m
        },
        new() {
            Id = 4,
            Title = "Test 4",
            Status = IncidentStatus.InProgress,
            CreatedAt = DateTime.UtcNow,
            Latitude = 4.0m,
            Longitude = 4.0m
        }
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var capturedRequest = (HttpRequestMessage)null;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains($"api/incident/user/{userId}")),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
                {
                    capturedRequest = request;
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(testIncidents, jsonOptions),
                        System.Text.Encoding.UTF8,
                        "application/json")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://test.com/")
            };

            _mockHttpClientFactory.Setup(x => x.CreateClient("AuthenticatedClient")).Returns(httpClient);

            // Act
            await _viewModel.LoadIncidentsAsync();

            // Assert
            Assert.That(capturedRequest, Is.Not.Null, "HTTP request should have been made");
            Assert.That(capturedRequest.RequestUri.ToString(), Does.Contain($"api/incident/user/{userId}"),
                "Request should be to the correct endpoint");

            Assert.That(_viewModel.Incidents, Is.Not.Null, "Incidents collection should not be null");
            Assert.That(_viewModel.Incidents.Count, Is.EqualTo(4), "Should have 4 incidents loaded");
            Assert.That(_viewModel.ActiveIncidentsCount, Is.EqualTo(3), "Should have 3 active incidents");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Test]
        public void ActiveIncidentsCount_ReturnsCorrectCountBasedOnStatus()
        {
            // Arrange
            var incidents = new ObservableCollection<IncidentDto>
            {
                new() { Status = IncidentStatus.Reported },
                new() { Status = IncidentStatus.Registered },
                new() { Status = IncidentStatus.Resolved },
                new() { Status = IncidentStatus.InProgress },
                new() { Status = IncidentStatus.Reported }
            };

            _viewModel.Incidents = incidents;

            // Act & Assert
            Assert.That(_viewModel.ActiveIncidentsCount, Is.EqualTo(4));
        }
    }
}