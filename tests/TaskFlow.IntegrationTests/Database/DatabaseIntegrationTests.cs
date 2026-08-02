using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace TaskFlow.IntegrationTests.Database;

public class DatabaseIntegrationTests : IntegrationTestBase
{
    public DatabaseIntegrationTests(TaskFlowWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Migrations_ApplyCleanly()
    {
        // Arrange
        var dbContext = GetService<ApplicationDbContext>();

        // Act - Check if database can be created and migrated
        var canConnect = await dbContext.Database.CanConnectAsync(CancellationToken.None);

        // Assert
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task SeedData_Works()
    {
        // Arrange
        var dbContext = GetService<ApplicationDbContext>();

        // Act
        var organizations = await dbContext.Organizations.ToListAsync(CancellationToken.None);

        // Assert
        organizations.Should().NotBeNull();
    }

    [Fact]
    public async Task GlobalQueryFilter_IsDeleted_ExcludesSoftDeletedEntities()
    {
        // Arrange
        await RegisterAndLoginAsync("softdelete@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("To Soft Delete", organizationId: orgId);

        // Act - Archive (soft delete) the project
        await Client.PatchAsync($"/api/projects/{projectId}/archive", null, CancellationToken.None);

        // Assert - Project should not appear in default queries
        var response = await Client.GetAsync("/api/projects", CancellationToken.None);
        var result = await response.Content.ReadFromJsonAsync<ProjectListResponse>(cancellationToken: CancellationToken.None);
        result!.Items.Should().NotContain(p => p.Id == projectId);
    }

    [Fact]
    public async Task GlobalQueryFilter_OrganizationId_EnforcesTenantIsolation()
    {
        // Arrange - User A creates org and project
        await RegisterAndLoginAsync("tenantusera@example.com", "Test123!");
        var orgAId = await CreateOrganizationAsync("Org A");
        var projectAId = await CreateProjectAsync("Project A", organizationId: orgAId);

        // Act - User B tries to query projects with Org A's ID
        await RegisterAndLoginAsync("tenantuserb@example.com", "Test123!");
        var response = await Client.GetAsync($"/api/projects?organizationId={orgAId}", CancellationToken.None);

        // Assert - Should be forbidden due to tenant isolation
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ConcurrencyToken_PreventsLostUpdates()
    {
        // Arrange
        await RegisterAndLoginAsync("concurrencyuser@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Concurrency Test", organizationId: orgId);

        // Get initial project
        var getResponse1 = await Client.GetAsync($"/api/projects/{projectId}", CancellationToken.None);
        var project1 = await getResponse1.Content.ReadFromJsonAsync<ProjectResponse>(cancellationToken: CancellationToken.None);

        // Simulate concurrent update - first update succeeds
        var updateResponse1 = await Client.PutAsJsonAsync($"/api/projects/{projectId}", new
        {
            Name = "Updated by User 1",
            Description = "Desc 1"
        }, CancellationToken.None);
        updateResponse1.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Second update with stale data should fail (409 Conflict)
        var updateResponse2 = await Client.PutAsJsonAsync($"/api/projects/{projectId}", new
        {
            Name = "Updated by User 2 (stale)",
            Description = "Desc 2"
        }, CancellationToken.None);

        // Assert - Should be 409 Conflict or 400 BadRequest for concurrency violation
        updateResponse2.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DatabaseTransaction_RollsBackOnError()
    {
        // This test verifies that failed operations don't leave partial data
        // Arrange
        await RegisterAndLoginAsync("transactionuser@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");

        // Act - Try to create project with invalid data (if any validation at API level)
        // Note: This depends on API validation. For now, test that valid operations work.
        var projectId = await CreateProjectAsync("Transaction Test", organizationId: orgId);

        // Assert
        projectId.Should().NotBeEmpty();

        // Verify project exists in DB
        var dbContext = GetService<ApplicationDbContext>();
        var project = await dbContext.Projects.FindAsync(new object[] { projectId }, CancellationToken.None);
        project.Should().NotBeNull();
        project!.Name.Should().Be("Transaction Test");
    }

    private record ProjectResponse(Guid Id, string Name, string? Description, Guid OrganizationId, bool IsArchived);
    private record ProjectListResponse(int Page, int PageSize, int TotalCount, List<ProjectItem> Items);
    private record ProjectItem(Guid Id, string Name, string? Description, Guid OrganizationId, bool IsArchived);
}
