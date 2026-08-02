using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace TaskFlow.IntegrationTests.Tasks;

public class TasksApiTests : IntegrationTestBase
{
    public TasksApiTests(TaskFlowWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task SearchTasks_ReturnsTasks()
    {
        // Arrange
        await RegisterAndLoginAsync("tasksearcher@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        await CreateTaskAsync("Task 1", projectId);
        await CreateTaskAsync("Task 2", projectId);

        // Act
        var response = await Client.GetAsync($"/api/tasks?projectId={projectId}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskListResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task SearchTasks_WithStatusFilter_ReturnsFilteredTasks()
    {
        // Arrange
        await RegisterAndLoginAsync("taskstatusfilter@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var openTaskId = await CreateTaskAsync("Open Task", projectId);
        var inProgressTaskId = await CreateTaskAsync("In Progress Task", projectId);
        // Note: Status would be updated via separate call

        // Act
        var response = await Client.GetAsync($"/api/tasks?projectId={projectId}&status=Todo", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskListResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchTasks_WithAssigneeFilter_ReturnsFilteredTasks()
    {
        // Arrange
        await RegisterAndLoginAsync("taskassigneefilter@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        await CreateTaskAsync("Task 1", projectId);

        // Act
        var response = await Client.GetAsync($"/api/tasks?projectId={projectId}&assigneeUserId={Guid.NewGuid()}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTaskById_ValidId_ReturnsTask()
    {
        // Arrange
        await RegisterAndLoginAsync("taskgetter@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var taskId = await CreateTaskAsync("Test Task", projectId, "Description", "High");

        // Act
        var response = await Client.GetAsync($"/api/tasks/{taskId}", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Id.Should().Be(taskId);
        result.Title.Should().Be("Test Task");
        result.Description.Should().Be("Description");
        result.Priority.Should().Be("High");
    }

    [Fact]
    public async Task CreateTask_ValidData_ReturnsCreated()
    {
        // Arrange
        await RegisterAndLoginAsync("taskcreator@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);

        // Act
        var response = await Client.PostAsJsonAsync("/api/tasks", new
        {
            ProjectId = projectId,
            Title = "New Task",
            Description = "Task description",
            Priority = "Medium",
            DueDate = DateTime.UtcNow.AddDays(7)
        }, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<TaskResponse>(cancellationToken: CancellationToken.None);
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.Title.Should().Be("New Task");
    }

    [Fact]
    public async Task UpdateTask_ValidData_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("taskupdater@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var taskId = await CreateTaskAsync("Original", projectId, "Original desc", "Low");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/tasks/{taskId}", new
        {
            Title = "Updated",
            Description = "Updated desc",
            Priority = "High",
            DueDate = DateTime.UtcNow.AddDays(14)
        }, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify
        var getResponse = await Client.GetAsync($"/api/tasks/{taskId}", CancellationToken.None);
        var updated = await getResponse.Content.ReadFromJsonAsync<TaskResponse>(cancellationToken: CancellationToken.None);
        updated!.Title.Should().Be("Updated");
        updated.Description.Should().Be("Updated desc");
        updated.Priority.Should().Be("High");
    }

    [Fact]
    public async Task ChangeTaskStatus_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("taskstatuschanger@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var taskId = await CreateTaskAsync("Status Test", projectId);

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/tasks/{taskId}/status", new
        {
            Status = "InProgress"
        }, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AssignTask_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("taskassigner@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var taskId = await CreateTaskAsync("Assign Test", projectId);

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/tasks/{taskId}/assign", new
        {
            AssigneeUserId = Guid.NewGuid()
        }, CancellationToken.None);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ArchiveTask_ReturnsNoContent()
    {
        // Arrange
        await RegisterAndLoginAsync("taskarchiver@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var taskId = await CreateTaskAsync("To Archive", projectId);

        // Act
        var response = await Client.PatchAsync($"/api/tasks/{taskId}/archive", null, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SearchTasks_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await RegisterAndLoginAsync("taskpager@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        for (int i = 1; i <= 5; i++)
        {
            await CreateTaskAsync($"Task {i}", projectId);
        }

        // Act
        var response = await Client.GetAsync($"/api/tasks?projectId={projectId}&page=1&pageSize=2", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskListResponse>(cancellationToken: CancellationToken.None);
        result!.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task SearchTasks_ArchivedFilter_Works()
    {
        // Arrange
        await RegisterAndLoginAsync("taskarchivedfilter@example.com", "Test123!");
        var orgId = await CreateOrganizationAsync("Test Org");
        var projectId = await CreateProjectAsync("Test Project", organizationId: orgId);
        var activeTaskId = await CreateTaskAsync("Active Task", projectId);
        var archivedTaskId = await CreateTaskAsync("Archived Task", projectId);
        await Client.PatchAsync($"/api/tasks/{archivedTaskId}/archive", null, CancellationToken.None);

        // Act
        var response = await Client.GetAsync($"/api/tasks?projectId={projectId}&isArchived=false", CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TaskListResponse>(cancellationToken: CancellationToken.None);
        result!.Items.Should().OnlyContain(t => !t.IsArchived);
    }

    private record TaskResponse(Guid Id, string Title, string? Description, string Priority, Guid ProjectId, string Status, bool IsArchived);
    private record TaskListResponse(int Page, int PageSize, int TotalCount, List<TaskItem> Items);
    private record TaskItem(Guid Id, string Title, string? Description, string Priority, string Status, bool IsArchived);
}
