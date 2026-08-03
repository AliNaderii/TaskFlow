using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace TaskFlow.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<TaskFlowWebApplicationFactory>, IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly TaskFlowWebApplicationFactory Factory;
    protected IServiceScope Scope => _scope ??= Factory.Services.CreateScope();
    private IServiceScope? _scope;

    protected IntegrationTestBase(TaskFlowWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    protected TService GetService<TService>() where TService : notnull
    {
        return Scope.ServiceProvider.GetRequiredService<TService>();
    }

    protected async Task<string> RegisterAndLoginAsync(string email = "test@example.com", string password = "Test123!")
    {
        var request = new
        {
            Email = email,
            Password = password,
            DisplayName = "Test User"
        };
        
        // Use TestContext to write output that will be captured
        System.Console.WriteLine($"[DEBUG] Register request: {System.Text.Json.JsonSerializer.Serialize(request)}");
        System.Console.WriteLine($"[DEBUG] Register URL: /api/authentication/register");
        
        var registerResponse = await Client.PostAsJsonAsync("/api/authentication/register", request);

        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        System.Console.WriteLine($"[DEBUG] Register status: {registerResponse.StatusCode} ({(int)registerResponse.StatusCode})");
        System.Console.WriteLine($"[DEBUG] Register content: {registerContent}");
        System.Console.WriteLine($"[DEBUG] Register headers: {string.Join(", ", registerResponse.Headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}"))}");

        registerResponse.EnsureSuccessStatusCode();

        var loginRequest = new
        {
            Email = email,
            Password = password
        };
        
        System.Console.WriteLine($"[DEBUG] Login request: {System.Text.Json.JsonSerializer.Serialize(loginRequest)}");
        System.Console.WriteLine($"[DEBUG] Login URL: /api/authentication/login");
        
        var loginResponse = await Client.PostAsJsonAsync("/api/authentication/login", loginRequest);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        System.Console.WriteLine($"[DEBUG] Login status: {loginResponse.StatusCode} ({(int)loginResponse.StatusCode})");
        System.Console.WriteLine($"[DEBUG] Login content: {loginContent}");
        System.Console.WriteLine($"[DEBUG] Login headers: {string.Join(", ", loginResponse.Headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}"))}");

        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        
        System.Console.WriteLine($"[DEBUG] LoginResult: UserId={loginResult!.UserId}, Token length={loginResult.Token.Length}");
        
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);
        
        System.Console.WriteLine($"[DEBUG] Authorization header set: {Client.DefaultRequestHeaders.Authorization}");
        
        return loginResult.Token;
    }

    protected async Task<Guid> CreateOrganizationAsync(string name = "Test Organization")
    {
        var response = await Client.PostAsJsonAsync("/api/organizations", new
        {
            Name = name
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<OrganizationResponse>();
        
        return result!.Id;
    }

    protected async Task<Guid> CreateProjectAsync(string name = "Test Project", string? description = null, Guid? organizationId = null)
    {
        var response = await Client.PostAsJsonAsync("/api/projects", new
        {
            Name = name,
            Description = description
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ProjectResponse>();
        
        return result!.Id;
    }

    protected async Task<Guid> CreateTaskAsync(string title = "Test Task", Guid? projectId = null, string? description = null, string priority = "Medium")
    {
        var response = await Client.PostAsJsonAsync("/api/tasks", new
        {
            ProjectId = projectId,
            Title = title,
            Description = description,
            Priority = priority
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TaskResponse>();
        
        return result!.Id;
    }

    protected async Task<Guid> CreateCommentAsync(string content = "Test Comment", Guid? taskId = null)
    {
        var response = await Client.PostAsJsonAsync($"/api/tasks/{taskId}/comments", new
        {
            Content = content
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CommentResponse>();
        
        return result!.Id;
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        if (_scope != null)
        {
            _scope.Dispose();
            _scope = null;
        }
        Client.Dispose();
    }

    private record LoginResponse(Guid UserId, string Token, string NewRefreshToken);
    private record OrganizationResponse(Guid Id, string Name);
    private record ProjectResponse(Guid Id, string Name);
    private record TaskResponse(Guid Id, string Title);
    private record CommentResponse(Guid Id, string Content);
}
