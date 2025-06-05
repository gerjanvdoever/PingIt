using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using PingIt.Maui.Dtos;
using PingIt.Maui.Services;
using PingIt.Maui.ViewModels;

namespace PingIt.Tests.Maui
{
    [TestFixture]
    public class LoginViewModelTests
    {
        private Mock<ITokenStorageService> _mockTokenStorage;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private Mock<ILogger<LoginViewModel>> _mockLogger;
        private LoginViewModel _viewModel;
        private HttpClient _httpClient;

        [SetUp]
        public void Setup()
        {
            _mockTokenStorage = new Mock<ITokenStorageService>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockLogger = new Mock<ILogger<LoginViewModel>>();

            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://test.com/")
            };

            _mockHttpClientFactory.Setup(x => x.CreateClient("PingItClient"))
                .Returns(_httpClient);

            _viewModel = new LoginViewModel(
                _mockTokenStorage.Object,
                _mockHttpClientFactory.Object,
                _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
        }

        [Test]
        public async Task LoginAsync_WithValidCredentials_StoresTokenAndNavigates()
        {
            // Arrange
            _viewModel.Email = "test@example.com";
            _viewModel.Password = "password123";

            var loginResponse = new LoginResponseDto
            {
                Token = "test-token",
                Role = "User"
            };

            var responseContent = new StringContent(
                JsonSerializer.Serialize(loginResponse),
                Encoding.UTF8,
                "application/json");

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("api/auth/login")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            _mockTokenStorage.Setup(x => x.StoreTokenAsync("test-token", "User"))
                .Returns(Task.CompletedTask);

            // Act
            await _viewModel.LoginAsync();

            // Assert
            Assert.That(_viewModel.IsBusy, Is.False);

            _mockTokenStorage.Verify(x => x.StoreTokenAsync("test-token", "User"), Times.Once);
        }

        [Test]
        public async Task LoginAsync_WithEmptyFields_ShowsValidationError()
        {
            // Arrange
            _viewModel.Email = "";
            _viewModel.Password = "";

            // Act
            await _viewModel.LoginAsync();

            // Assert
            Assert.That(_viewModel.IsBusy, Is.False);
            Assert.That(_viewModel.ValidationError, Is.EqualTo("Please fill in all required fields"));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task LoginAsync_WhenServerReturnsError_ShowsErrorMessage()
        {
            // Arrange
            _viewModel.Email = "test@example.com";
            _viewModel.Password = "password123";

            var errorResponse = new Dictionary<string, string>
            {
                { "Message", "Invalid credentials" }
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(
                        JsonSerializer.Serialize(errorResponse),
                        System.Text.Encoding.UTF8,
                        "application/json")
                });

            // Act
            await _viewModel.LoginAsync();

            // Assert
            Assert.That(_viewModel.IsBusy, Is.False);
            Assert.That(_viewModel.ValidationError, Is.EqualTo("Invalid credentials"));
        }

        [Test]
        public async Task LoginAsync_ReturnsError_ShowsMessage()
        {
            // Arrange
            _viewModel.Email = "test@example.com";
            _viewModel.Password = "password123";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Server error")
                });

            // Act
            await _viewModel.LoginAsync();

            // Assert
            Assert.That(_viewModel.IsBusy, Is.False);
            Assert.That(_viewModel.ValidationError, Is.EqualTo("Login failed"));
        }

        [Test]
        public async Task LoginAsync_WhenResponseIsInvalid_ShowsErrorMessage()
        {
            // Arrange
            _viewModel.Email = "test@example.com";
            _viewModel.Password = "password123";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("invalid-json")
                });

            // Act
            await _viewModel.LoginAsync();

            // Assert
            Assert.That(_viewModel.IsBusy, Is.False);
            Assert.That(_viewModel.ValidationError, Is.EqualTo("Didn't receive valid token"));
        }

        [Test]
        public async Task LoginAsync_LogsErrorAndShowsMessage()
        {
            // Arrange
            _viewModel.Email = "test@example.com";
            _viewModel.Password = "password123";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            await _viewModel.LoginAsync();

            // Assert
            Assert.That(_viewModel.IsBusy, Is.False);
            Assert.That(_viewModel.ValidationError, Is.EqualTo("Something went wrong when trying to log in."));

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Login error")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public void LoginCommand_WhenIsBusy_CannotExecute()
        {
            // Arrange
            _viewModel.IsBusy = true;

            // Act & Assert
            Assert.That(_viewModel.LoginCommand.CanExecute(null), Is.False);
        }
    }
}