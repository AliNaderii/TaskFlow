using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace TaskFlow.IntegrationTests.Projects;

public class ProjectsApiTests : IntegrationTestBase
{
    public ProjectsApiTests(TaskFlowWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task SearchProjects_ReturnsProjects()
    {
        // Arrange
        await RegisterAndLoginAsync("projectsearcher@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        await CreateProjectAsync("Project 1", organizationId: orgId);
        await CreateProjectAsync("Project 2", organizationId: orgId);

        // Act
        var response = await Client.GetAsync("/api/projects", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ProjectListResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task SearchProjects_WithKeyword_FiltersResults()
    {
        // Arrange
        await RegisterAndLoginAsync("projectfilter@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        await CreateProjectAsync("Alpha Project", organizationId: orgId);
        await CreateProjectAsync("Beta Project", organizationId: orgId);

        // Act
        var response = await Client.GetAsync("/api/projects?keyword=Alpha", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ProjectListResponse>(cancellationToken: CancellationToken.None);
        result!.Items.Should().OnlyContain(p => p.Name.Contains("Alpha"));
    }

    [Fact]
    public async Task GetProjectById_ValidId_ReturnsProject()
    {
        // Arrange
        await RegisterAndLoginAsync("projectgetter@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", "Description", orgId);

        // Act
        var response = await Client.GetAsync($"/api/projects/{projectId}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ProjectResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Id.Should().Be(projectId);
        result.Name.Should().Be("Test Project");
        result.Description.Should().Be("Description");
    }

    [Fact]
    public async Task CreateProject_ValidData_ReturnsCreated()
    {
        // Arrange
        await RegisterAndLoginAsync("projectcreator@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");

        // Act
        var response = await Client.PostAsJsonAsync("/api/projects", new
        {
            Name = "New Project",
            Description = "Project description",
            OrganizationId = orgId
        }, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ProjectResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.Name.Should().Be("New Project");
    }

    [Fact]
    public async Task UpdateProject_ValidData_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("projectupdater@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Original", "Original desc", orgId);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/projects/{projectId}", new
        {
            Name = "Updated",
            Description = "Updated desc"
        }, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify
        var getResponse = await Client.GetAsync($"/api/projects/{projectId}", CancellationToken.None);
        var updated = await getResponse.Content.ReadFromJsonAsync<ProjectResponse>(cancellationToken: CancellationToken.None);
        updated!.Name.Should().Be("Updated");
        updated.Description.Should().Be("Updated desc");
    }

    [Fact]
    public async Task ArchiveProject_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("projectarchiver@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("To Archive", organizationId: orgId);

        // Act
        var response = await Client.PatchAsync($"/api/projects/{projectId}/archive", null, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SearchProjects_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await RegisterAndLoginAsync("projectpager@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        for (int i = 1; i <= 5; i++)
        {
            await CreateProjectAsync($"Project {i}", organizationId: orgId);
        }

        // Act
        var response = await Client.GetAsync("/api/projects?page=1&pageSize=2", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ProjectListResponse>(cancellationToken: CancellationToken.None);
        result!.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task SearchProjects_ArchivedFilter_Works()
    {
        // Arrange
        await RegisterAndLoginAsync("projectarchivedfilter@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var activeProjectId = await CreateProjectAsync("Active Project", organizationId: orgId);
        var archivedProjectId = await CreateProjectAsync("Archived Project", organizationId: orgId);
        await Client.PatchAsync($"/api/projects/{archivedProjectId}/archive", null, CancellationToken.None);

        // Act - search only active
        var response = await Client.GetAsync("/api/projects?isArchived=false", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ProjectListResponse>(cancellationToken: CancellationToken.None);
        result!.Items.Should().OnlyContain(p => !p.IsArchived);
    }

    private record ProjectResponse(Guid Id, string Name, string? Description, Guid OrganizationId, bool IsArchived);
    private record ProjectListResponse(int Page, int PageSize, int TotalCount, List<ProjectItem> Items);
    private record ProjectItem(Guid Id, string Name, string? Description, Guid OrganizationId, bool IsArchived);
}
