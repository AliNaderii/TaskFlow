using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace TaskFlow.IntegrationTests.Comments;

public class CommentsApiTests : IntegrationTestBase
{
    public CommentsApiTests(TaskFlowWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateComment_ValidData_ReturnsCreated()
    {
        // Arrange
        await RegisterAndLoginAsync("commentcreator@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var taskId = await CreateTaskAsync("Test Task", projectId);

        // Act
        var response = await Client.PostAsJsonAsync($"/api/tasks/{taskId}/comments", new
        {
            Content = "New comment"
        }, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CommentResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.Content.Should().Be("New comment");
    }

    [Fact]
    public async Task GetCommentById_ValidId_ReturnsComment()
    {
        // Arrange
        await RegisterAndLoginAsync("commentgetter@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var taskId = await CreateTaskAsync("Test Task", projectId);
        var commentId = await CreateCommentAsync("Test Comment", taskId);

        // Act
        var response = await Client.GetAsync($"/api/tasks/{taskId}/comments/{commentId}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CommentResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Id.Should().Be(commentId);
        result.Content.Should().Be("Test Comment");
    }

    [Fact]
    public async Task GetCommentsByTask_ReturnsComments()
    {
        // Arrange
        await RegisterAndLoginAsync("commentlister@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var taskId = await CreateTaskAsync("Test Task", projectId);
        await CreateCommentAsync("Comment 1", taskId);
        await CreateCommentAsync("Comment 2", taskId);

        // Act
        var response = await Client.GetAsync($"/api/tasks/{taskId}/comments", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CommentListResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task UpdateComment_ValidData_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("commentupdater@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var taskId = await CreateTaskAsync("Test Task", projectId);
        var commentId = await CreateCommentAsync("Original", taskId);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/tasks/{taskId}/comments/{commentId}", new
        {
            Content = "Updated comment"
        }, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify
        var getResponse = await Client.GetAsync($"/api/tasks/{taskId}/comments/{commentId}", CancellationToken.None);
        var updated = await getResponse.Content.ReadFromJsonAsync<CommentResponse>(cancellationToken: CancellationToken.None);
        updated!.Content.Should().Be("Updated comment");
    }

    [Fact]
    public async Task DeleteComment_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("commentdeleter@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var taskId = await CreateTaskAsync("Test Task", projectId);
        var commentId = await CreateCommentAsync("To Delete", taskId);

        // Act
        var response = await Client.DeleteAsync($"/api/tasks/{taskId}/comments/{commentId}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SearchComments_ReturnsComments()
    {
        // Arrange
        await RegisterAndLoginAsync("commentsearcher@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var taskId = await CreateTaskAsync("Test Task", projectId);
        await CreateCommentAsync("First comment", taskId);
        await CreateCommentAsync("Second comment", taskId);

        // Act
        var response = await Client.GetAsync("/api/comments?keyword=First", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CommentSearchResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Items.Should().OnlyContain(c => c.Content.Contains("First"));
    }

    [Fact]
    public async Task SearchComments_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await RegisterAndLoginAsync("commentpager@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var taskId = await CreateTaskAsync("Test Task", projectId);
        for (int i = 1; i <= 5; i++)
        {
            await CreateCommentAsync($"Comment {i}", taskId);
        }

        // Act
        var response = await Client.GetAsync("/api/comments?page=1&pageSize=2", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CommentSearchResponse>(cancellationToken: CancellationToken.None);
        result!.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task SearchComments_WithTaskIdFilter_ReturnsFilteredComments()
    {
        // Arrange
        await RegisterAndLoginAsync("commenttaskfilter@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var task1Id = await CreateTaskAsync("Task 1", projectId);
        var task2Id = await CreateTaskAsync("Task 2", projectId);
        await CreateCommentAsync("Comment on Task 1", task1Id);
        await CreateCommentAsync("Comment on Task 2", task2Id);

        // Act
        var response = await Client.GetAsync($"/api/comments?taskId={task1Id}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CommentSearchResponse>(cancellationToken: CancellationToken.None);
        result!.Items.Should().OnlyContain(c => c.TaskId == task1Id);
    }

    private record CommentResponse(Guid Id, string Content, Guid TaskId, Guid AuthorId, DateTime CreatedAt);
    private record CommentListResponse(List<CommentResponse> Items);
    private record CommentSearchResponse(int Page, int PageSize, int TotalCount, List<CommentSearchItem> Items);
    private record CommentSearchItem(Guid Id, string Content, Guid TaskId, Guid AuthorId, DateTime CreatedAt);
}
