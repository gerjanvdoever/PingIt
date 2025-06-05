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
                new() { Status = IncidentStatus.Reported },
                new() { Status = IncidentStatus.Registered },
                new() { Status = IncidentStatus.Resolved }, // Shouldn't count as active
                new() { Status = IncidentStatus.InProgress }
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(testIncidents))
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient("AuthenticatedClient")).Returns(httpClient);

            // Act
            await _viewModel.LoadIncidentsAsync();

            // Assert
            Assert.That(_viewModel.ActiveIncidentsCount, Is.EqualTo(3));
            Assert.That(_viewModel.Incidents.Count, Is.EqualTo(4));
            _mockLogger.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>()),
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
                new() { Status = IncidentStatus.Resolved }, // Not active
                new() { Status = IncidentStatus.InProgress },
                new() { Status = IncidentStatus.Reported }
            };

            _viewModel.Incidents = incidents;

            // Act & Assert
            Assert.That(_viewModel.ActiveIncidentsCount, Is.EqualTo(4));
        }
    }
}