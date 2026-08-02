using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace TaskFlow.IntegrationTests.MultiTenancy;

public class TenantIsolationTests : IntegrationTestBase
{
    public TenantIsolationTests(TaskFlowWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UserInOrgA_CannotAccessOrgBProjects()
    {
        // Arrange - User A creates org and project
        await RegisterAndLoginAsync("usera@example.com", "Test123!");
        var orgAId = await CreateOrganizationAsync("Org A");
        var projectAId = await CreateProjectAsync("Project A", organizationId: orgAId);

        // Act - User B tries to access Org A's project
        await RegisterAndLoginAsync("userb@example.com", "Test123!");
        var response = await Client.GetAsync($"/api/projects/{projectAId}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UserInOrgA_CannotAccessOrgBTasks()
    {
        // Arrange - User A creates org, project, task
        await RegisterAndLoginAsync("usera2@example.com", "Test123!");
        var orgAId = await CreateOrganizationAsync("Org A2");
        var projectAId = await CreateProjectAsync("Project A2", organizationId: orgAId);
        var taskAId = await CreateTaskAsync("Task A2", projectAId);

        // Act - User B tries to access Org A's task
        await RegisterAndLoginAsync("userb2@example.com", "Test123!");
        var response = await Client.GetAsync($"/api/tasks/{taskAId}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UserInOrgA_CannotAccessOrgBComments()
    {
        // Arrange - User A creates org, project, task, comment
        await RegisterAndLoginAsync("usera3@example.com", "Test123!");
        var orgAId = await CreateOrganizationAsync("Org A3");
        var projectAId = await CreateProjectAsync("Project A3", organizationId: orgAId);
        var taskAId = await CreateTaskAsync("Task A3", projectAId);
        var commentId = await CreateCommentAsync("Comment A3", taskAId);

        // Act - User B tries to access Org A's comment
        await RegisterAndLoginAsync("userb3@example.com", "Test123!");
        var response = await Client.GetAsync($"/api/tasks/{taskAId}/comments/{commentId}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UserInOrgA_CannotListOrgBProjects()
    {
        // Arrange - User A creates org and projects
        await RegisterAndLoginAsync("usera4@example.com", "Test123!");
        var orgAId = await CreateOrganizationAsync("Org A4");
        await CreateProjectAsync("Project A4-1", organizationId: orgAId);
        await CreateProjectAsync("Project A4-2", organizationId: orgAId);

        // Act - User B tries to list Org A's projects
        await RegisterAndLoginAsync("userb4@example.com", "Test123!");
        var response = await Client.GetAsync($"/api/projects?organizationId={orgAId}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UserInOrgA_CannotAcceptOrgBInvitation()
    {
        // Arrange - User A creates org and sends invitation
        await RegisterAndLoginAsync("usera5@example.com", "Test123!");
        var orgAId = await CreateOrganizationAsync("Org A5");
        var inviteResponse = await Client.PostAsJsonAsync($"/api/organizations/{orgAId}/invitations", new
        {
            Email = "invitee@example.com",
            Role = "Member",
            ExpirationDays = 7
        }, CancellationToken.None);

        inviteResponse.EnsureSuccessStatusCode();
        var inviteResult = await inviteResponse.Content.ReadFromJsonAsync<InvitationResponse>(cancellationToken: CancellationToken.None);

        // Act - User B (different from invitee) tries to accept
        await RegisterAndLoginAsync("userb5@example.com", "Test123!");
        var response = await Client.PostAsJsonAsync("/api/organizations/invitations/accept", new
        {
            Token = inviteResult!.Token
        }, CancellationToken.None);

        // Assert - should fail (wrong user accepting)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TenantResolution_WorksViaUserContext()
    {
        // Arrange
        await RegisterAndLoginAsync("tenantuser@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Tenant Org");
        var projectId = await CreateProjectAsync("Tenant Project", organizationId: orgId);

        // Act - Access own resources without explicit orgId
        var response = await Client.GetAsync("/api/projects", CancellationToken.None);

        // Assert - Should only see projects from user's organization
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var projects = await response.Content.ReadFromJsonAsync<ProjectListResponse>(cancellationToken: CancellationToken.None);
        projects.Should().NotBeNull();
        projects!.Items.Should().Contain(p => p.Id == projectId);
        projects.Items.Should().OnlyContain(p => p.OrganizationId == orgId);
    }

    private async Task<Guid> CreateTaskAsync(string title, Guid projectId)
    {
        var response = await Client.PostAsJsonAsync("/api/tasks", new
        {
            ProjectId = projectId,
            Title = title,
            Description = "Test description",
            Priority = "Medium"
        }, CancellationToken.None);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TaskResponse>(cancellationToken: CancellationToken.None);
        return result!.Id;
    }

    private async Task<Guid> CreateCommentAsync(string content, Guid taskId)
    {
        var response = await Client.PostAsJsonAsync($"/api/tasks/{taskId}/comments", new
        {
            Content = content
        }, CancellationToken.None);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CommentResponse>(cancellationToken: CancellationToken.None);
        return result!.Id;
    }

    private record TaskResponse(Guid Id, string Title);
    private record CommentResponse(Guid Id, string Content);
    private record InvitationResponse(string Token, string Email, string Role);
    private record ProjectListResponse(List<ProjectItem> Items);
    private record ProjectItem(Guid Id, string Name, Guid OrganizationId);
}
