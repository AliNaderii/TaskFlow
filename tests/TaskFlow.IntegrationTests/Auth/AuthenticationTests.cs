using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace TaskFlow.IntegrationTests.Auth;

public class AuthenticationTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;

    public AuthenticationTests(TaskFlowWebApplicationFactory factory, ITestOutputHelper output) : base(factory)
    {
        _output = output;
    }

    [Fact]
    public async Task Register_ValidData_ReturnsUserId()
    {
        // Act
        var request = new
        {
            Email = "newuser@example.com",
            Password = "Test123!",
            DisplayName = "New User"
        };
        
        // Check if ANY routes exist - test a few different ones
        var routesToTest = new[] 
        { 
            "/", 
            "/swagger", 
            "/swagger/v1/swagger.json",
            "/api/authentication",
            "/api/authentication/register",
            "/api/authentication/login",
            "/api/authentication/refresh-token",
            "/api/organizations",
            "/api/projects"
        };
        
        foreach (var route in routesToTest)
        {
            var r = await Client.GetAsync(route);
            var c = await r.Content.ReadAsStringAsync();
            _output.WriteLine($"=== ROUTE {route} ===");
            _output.WriteLine($"Status Code: {r.StatusCode} ({(int)r.StatusCode})");
            _output.WriteLine($"Content: {c}");
            _output.WriteLine($"============================");
        }
        
        var response = await Client.PostAsJsonAsync("/api/authentication/register", request);

        // Debug: output status code and content
        var content = await response.Content.ReadAsStringAsync();
        var statusCode = response.StatusCode;
        
        _output.WriteLine($"=== REGISTER TEST DEBUG ===");
        _output.WriteLine($"Status Code: {statusCode} ({(int)statusCode})");
        _output.WriteLine($"Content: {content}");
        _output.WriteLine($"Request: {System.Text.Json.JsonSerializer.Serialize(request)}");
        _output.WriteLine($"============================");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue($"Status: {(int)statusCode}, Content: {content}");
        
        var result = System.Text.Json.JsonSerializer.Deserialize<RegisterResponse>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
        });

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
        });

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
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
        });

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
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var refreshToken = loginResult!.NewRefreshToken!;

        // Act
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PostAsJsonAsync("/api/authentication/refresh-token", new
        {
            RefreshToken = refreshToken
        });

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
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
        });

        // Assert - returns BadRequest (400) with error, not Unauthorized (401)
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    private record RegisterResponse(Guid Id);
    private record LoginResponse(Guid UserId, string Token, string? NewRefreshToken);
}
