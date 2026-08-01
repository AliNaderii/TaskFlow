using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace TaskFlow.IntegrationTests.Auth;

public class AuthenticationTests : IntegrationTestBase
{
    public AuthenticationTests(TaskFlowWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_ValidData_ReturnsUserId()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/authentication/register", new
        {
            Email = "newuser@example.com",
            Password = "Test123!",
            DisplayName = "New User"
        }, TestContext.Current.CancellationToken);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>(cancellationToken: TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        await RegisterAndLoginAsync("duplicate@example.com", "Test123!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/authentication/register", new
        {
            Email = "duplicate@example.com",
            Password = "Test123!",
            DisplayName = "Another User"
        }, TestContext.Current.CancellationToken);

        // Assert - returns BadRequest (400) with validation error, not Conflict (409)
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        // Arrange
        await RegisterAndLoginAsync("login@example.com", "Test123!");

        // Act - logout first
        Client.DefaultRequestHeaders.Authorization = null;
        
        var response = await Client.PostAsJsonAsync("/api/authentication/login", new
        {
            Email = "login@example.com",
            Password = "Test123!"
        }, TestContext.Current.CancellationToken);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result!.UserId.Should().NotBeEmpty();
        result.Token.Should().NotBeNullOrEmpty();
        result.NewRefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        await RegisterAndLoginAsync("wrongpass@example.com", "Test123!");
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync("/api/authentication/login", new
        {
            Email = "wrongpass@example.com",
            Password = "WrongPassword123!"
        }, TestContext.Current.CancellationToken);

        // Assert - returns BadRequest (400) with error, not Unauthorized (401)
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsNewTokens()
    {
        // Arrange
        await RegisterAndLoginAsync("refresh@example.com", "Test123!");
        var loginResponse = await Client.PostAsJsonAsync("/api/authentication/login", new
        {
            Email = "refresh@example.com",
            Password = "Test123!"
        }, TestContext.Current.CancellationToken);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: TestContext.Current.CancellationToken);
        var refreshToken = loginResult!.NewRefreshToken!;

        // Act
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PostAsJsonAsync("/api/authentication/refresh-token", new
        {
            RefreshToken = refreshToken
        }, TestContext.Current.CancellationToken);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result!.UserId.Should().NotBeEmpty();
        result.Token.Should().NotBeNullOrEmpty();
        result.NewRefreshToken.Should().NotBeNullOrEmpty();
        result.NewRefreshToken.Should().NotBe(refreshToken); // Token rotation
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_ReturnsBadRequest()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/authentication/refresh-token", new
        {
            RefreshToken = "invalid-token"
        }, TestContext.Current.CancellationToken);

        // Assert - returns BadRequest (400) with error, not Unauthorized (401)
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    private record RegisterResponse(Guid Id);
    private record LoginResponse(Guid UserId, string Token, string? NewRefreshToken);
}
