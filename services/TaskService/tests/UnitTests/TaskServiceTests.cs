using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Repositories;
using Moq;
using Task = Domain.Entities.Task;
using NetTask = System.Threading.Tasks.Task;
namespace UnitTests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IProjectServiceClient> _projectServiceClientMock;
    private readonly Mock<IUserServiceClient> _userServiceClientMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _projectServiceClientMock = new Mock<IProjectServiceClient>();
        _userServiceClientMock = new Mock<IUserServiceClient>();
        _mapperMock = new Mock<IMapper>();
        _taskService = new TaskService(
            _taskRepositoryMock.Object,
            _projectServiceClientMock.Object,
            _userServiceClientMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async NetTask CreateTaskAsync_Should_CreateTask_WhenProjectAndAssigneeExist()
    {
        // Arrange
        var request = new CreateTaskRequest(Guid.NewGuid(), "Test Task", "Description", Guid.NewGuid(), "ToDo");
        var task = new Task { Id = Guid.NewGuid(), ProjectId = request.ProjectId, Title = request.Title, Description = request.Description, AssigneeId = request.AssigneeId, Status = request.Status };
        var taskDto = new TaskDto { Id = task.Id, ProjectId = task.ProjectId, Title = task.Title, Description = task.Description, AssigneeId = task.AssigneeId, Status = task.Status };

        _projectServiceClientMock.Setup(c => c.ProjectExistsAsync(request.ProjectId)).ReturnsAsync(true);
        _userServiceClientMock.Setup(c => c.UserExistsAsync(request.AssigneeId!.Value)).ReturnsAsync(true);
        _mapperMock.Setup(m => m.Map<TaskDto>(It.IsAny<Task>())).Returns(taskDto);

        // Act
        var result = await _taskService.CreateTaskAsync(request);

        // Assert
        _taskRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Task>()), Times.Once());
        Assert.Equal(taskDto, result);
    }

    [Fact]
    public async NetTask CreateTaskAsync_Should_ThrowException_WhenProjectNotExists()
    {
        // Arrange
        var request = new CreateTaskRequest(Guid.NewGuid(), "Test Task", "Description", Guid.NewGuid(), "ToDo");
        _projectServiceClientMock.Setup(c => c.ProjectExistsAsync(request.ProjectId)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _taskService.CreateTaskAsync(request));
    }
}