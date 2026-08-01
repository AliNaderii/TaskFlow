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
        Client = factory.CreateClient();
    }

    protected TService GetService<TService>() where TService : notnull
    {
        return Scope.ServiceProvider.GetRequiredService<TService>();
    }

    protected async Task<string> RegisterAndLoginAsync(string email = "test@example.com", string password = "Test123!")
    {
        var registerResponse = await Client.PostAsJsonAsync("/api/authentication/register", new
        {
            Email = email,
            Password = password,
            DisplayName = "Test User"
        });

        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await Client.PostAsJsonAsync("/api/authentication/login", new
        {
            Email = email,
            Password = password
        });

        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
        
        return loginResult.AccessToken;
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

    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public virtual async ValueTask DisposeAsync()
    {
        if (_scope != null)
        {
            _scope.Dispose();
            _scope = null;
        }
        Client.Dispose();
    }

    private record LoginResponse(string AccessToken, string RefreshToken);
    private record OrganizationResponse(Guid Id, string Name);
    private record ProjectResponse(Guid Id, string Name);
}
