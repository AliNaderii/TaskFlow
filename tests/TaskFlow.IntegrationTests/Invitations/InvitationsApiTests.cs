using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace TaskFlow.IntegrationTests.Invitations;

public class InvitationsApiTests : IntegrationTestBase
{
    public InvitationsApiTests(TaskFlowWebApplicationFactory factory) : base(factory)
    {
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
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task AcceptInvitation_ValidToken_ReturnsSuccess()
    {
        // Arrange - User A creates org and invitation
        await RegisterAndLoginAsync("inviteowner@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var inviteResponse = await Client.PostAsJsonAsync($"/api/organizations/{orgId}/invitations", new
        {
            Email = "acceptor@example.com",
            Role = "Member",
            ExpirationDays = 7
        }, CancellationToken.None);
        var inviteResult = await inviteResponse.Content.ReadFromJsonAsync<InvitationResponse>(cancellationToken: CancellationToken.None);

        // Act - User B (the invitee) accepts - register and login first
        await RegisterAndLoginAsync("acceptor@example.com", "Test123!");
        Client.DefaultRequestHeaders.Authorization = null; // Clear auth for accept endpoint
        
        var response = await Client.PostAsJsonAsync("/api/organizations/invitations/accept", new
        {
            Token = inviteResult!.Token
        }, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AcceptInvitation_InvalidToken_ReturnsBadRequest()
    {
        // Act
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PostAsJsonAsync("/api/organizations/invitations/accept", new
        {
            Token = "invalid-token"
        }, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
        var result = await response.Content.ReadFromJsonAsync<InvitationDetailResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Email.Should().Be("invitee@example.com");
        result.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task CancelInvitation_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("invitecanceler@example.com", "Test123!");
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
    public async Task ListOrganizationInvitations_ReturnsList()
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
        await Client.PostAsJsonAsync($"/api/organizations/{orgId}/invitations", new
        {
            Email = "invitee2@example.com",
            Role = "Admin",
            ExpirationDays = 7
        }, CancellationToken.None);

        // Act
        var response = await Client.GetAsync($"/api/organizations/{orgId}/invitations", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<InvitationListResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    private record InvitationResponse(Guid Id, string Token, string Email, string Role, string Status, DateTime ExpiresAt, string InvitedBy);
    private record InvitationDetailResponse(Guid Id, string Token, string Email, string Role, string Status, DateTime ExpiresAt, string OrganizationName);
    private record InvitationListResponse(List<InvitationResponse> Items);
}
