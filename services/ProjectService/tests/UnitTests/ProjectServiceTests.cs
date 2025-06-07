using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using Moq;

namespace UnitTests;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUserServiceClient> _userServiceClientMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ProjectService _projectService;

    public ProjectServiceTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _userServiceClientMock = new Mock<IUserServiceClient>();
        _mapperMock = new Mock<IMapper>();
        _projectService = new ProjectService(
            _projectRepositoryMock.Object,
            _userServiceClientMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task CreateProjectAsync_Should_CreateProject_WhenUserExists()
    {
        // Arrange
        var request = new CreateProjectRequest("Test Project", "Description", Guid.NewGuid());
        var project = new Project { Id = Guid.NewGuid(), Name = request.Name, Description = request.Description, ProjectManagerId = request.ProjectManagerId };
        var projectDto = new ProjectDto { Id = project.Id, Name = project.Name, Description = project.Description, ProjectManagerId = project.ProjectManagerId };

        _userServiceClientMock.Setup(c => c.UserExistsAsync(request.ProjectManagerId)).ReturnsAsync(true);
        _mapperMock.Setup(m => m.Map<ProjectDto>(It.IsAny<Project>())).Returns(projectDto);

        // Act
        var result = await _projectService.CreateProjectAsync(request);

        // Assert
        _projectRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Once());
        Assert.Equal(projectDto, result);
    }

    [Fact]
    public async Task CreateProjectAsync_Should_ThrowException_WhenUserNotExists()
    {
        // Arrange
        var request = new CreateProjectRequest("Test Project", "Description", Guid.NewGuid());
        _userServiceClientMock.Setup(c => c.UserExistsAsync(request.ProjectManagerId)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _projectService.CreateProjectAsync(request));
    }
}