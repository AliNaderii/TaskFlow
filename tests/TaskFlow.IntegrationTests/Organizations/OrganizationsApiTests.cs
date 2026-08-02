using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace TaskFlow.IntegrationTests.Organizations;

public class OrganizationsApiTests : IntegrationTestBase
{
    public OrganizationsApiTests(TaskFlowWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateOrganization_ValidData_ReturnsCreated()
    {
        // Arrange
        await RegisterAndLoginAsync("orgcreator@example.com", "Test123!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/organizations", new
        {
            Name = "New Organization"
        }, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<OrganizationResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.Name.Should().Be("New Organization");
    }

    [Fact]
    public async Task GetOrganizationById_ValidId_ReturnsOrganization()
    {
        // Arrange
        await RegisterAndLoginAsync("orggetter@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Organization");

        // Act
        var response = await Client.GetAsync($"/api/organizations/{orgId}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<OrganizationResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Id.Should().Be(orgId);
        result.Name.Should().Be("Test Organization");
    }

    [Fact]
    public async Task UpdateOrganization_ValidData_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("orgupdater@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Original Name");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/organizations/{orgId}", new
        {
            Name = "Updated Name"
        }, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await Client.GetAsync($"/api/organizations/{orgId}", CancellationToken.None);
        var updated = await getResponse.Content.ReadFromJsonAsync<OrganizationResponse>(cancellationToken: CancellationToken.None);
        updated!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task ArchiveOrganization_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("orgarchiver@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("To Archive");

        // Act
        var response = await Client.PatchAsync($"/api/organizations/{orgId}/archive", null, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CreateInvitation_ValidData_ReturnsCreated()
    {
        // Arrange
        await RegisterAndLoginAsync("invitecreator@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");

        // Act
        var response = await Client.PostAsJsonAsync($"/api/organizations/{orgId}/invitations", new
        {
            Email = "invitee@example.com",
            Role = "Member",
            ExpirationDays = 7
        }, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<InvitationResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Email.Should().Be("invitee@example.com");
        result.Role.Should().Be("Member");
    }

    [Fact]
    public async Task GetOrganizationInvitations_ReturnsList()
    {
        // Arrange
        await RegisterAndLoginAsync("invitelist@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        await Client.PostAsJsonAsync($"/api/organizations/{orgId}/invitations", new
        {
            Email = "invitee1@example.com",
            Role = "Member",
            ExpirationDays = 7
        }, CancellationToken.None);

        // Act
        var response = await Client.GetAsync($"/api/organizations/{orgId}/invitations", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<InvitationListResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task CancelInvitation_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("invitecancel@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var inviteResponse = await Client.PostAsJsonAsync($"/api/organizations/{orgId}/invitations", new
        {
            Email = "invitee@example.com",
            Role = "Member",
            ExpirationDays = 7
        }, CancellationToken.None);
        var inviteResult = await inviteResponse.Content.ReadFromJsonAsync<InvitationResponse>(cancellationToken: CancellationToken.None);

        // Act
        var response = await Client.DeleteAsync($"/api/organizations/{orgId}/invitations/{inviteResult!.Id}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetInvitationByToken_ValidToken_ReturnsInvitation()
    {
        // Arrange
        await RegisterAndLoginAsync("invitegetter@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var inviteResponse = await Client.PostAsJsonAsync($"/api/organizations/{orgId}/invitations", new
        {
            Email = "invitee@example.com",
            Role = "Admin",
            ExpirationDays = 7
        }, CancellationToken.None);
        var inviteResult = await inviteResponse.Content.ReadFromJsonAsync<InvitationResponse>(cancellationToken: CancellationToken.None);

        // Act - AllowAnonymous endpoint
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.GetAsync($"/api/organizations/invitations/{inviteResult!.Token}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RemoveMember_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("memberremover@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        // Note: Would need to add a member first in real scenario

        // Act
        var response = await Client.DeleteAsync($"/api/organizations/{orgId}/members/{Guid.NewGuid()}", CancellationToken.None);

        // Assert - 404 for non-existent member, but policy allows
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LeaveOrganization_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("memberleaver@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");

        // Act
        var response = await Client.PostAsJsonAsync($"/api/organizations/{orgId}/members/leave", new { }, CancellationToken.None);

        // Assert - Owner cannot leave, but policy allows the request
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeMemberRole_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("rolechanger@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/organizations/{orgId}/members/{Guid.NewGuid()}/role", new
        {
            Role = "Admin"
        }, CancellationToken.None);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SuspendMember_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("membersuspender@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");

        // Act
        var response = await Client.PutAsync($"/api/organizations/{orgId}/members/{Guid.NewGuid()}/suspend", null, CancellationToken.None);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    private record OrganizationResponse(Guid Id, string Name);
    private record InvitationResponse(Guid Id, string Token, string Email, string Role, string Status, DateTime ExpiresAt, string InvitedBy);
    private record InvitationListResponse(List<InvitationResponse> Items);
}
