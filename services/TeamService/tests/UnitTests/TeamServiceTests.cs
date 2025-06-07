using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using Moq;

namespace UnitTests;

public class TeamServiceTests
{
    private readonly Mock<ITeamRepository> _teamRepositoryMock;
    private readonly Mock<IUserServiceClient> _userServiceClientMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly TeamService _teamService;

    public TeamServiceTests()
    {
        _teamRepositoryMock = new Mock<ITeamRepository>();
        _userServiceClientMock = new Mock<IUserServiceClient>();
        _mapperMock = new Mock<IMapper>();
        _teamService = new TeamService(
            _teamRepositoryMock.Object,
            _userServiceClientMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task CreateTeamAsync_Should_CreateTeam()
    {
        // Arrange
        var request = new CreateTeamRequest("Test Team");
        var team = new Team { Id = Guid.NewGuid(), Name = request.Name };
        var teamDto = new TeamDto { Id = team.Id, Name = team.Name };

        _mapperMock.Setup(m => m.Map<TeamDto>(It.IsAny<Team>())).Returns(teamDto);

        // Act
        var result = await _teamService.CreateTeamAsync(request);

        // Assert
        _teamRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Team>()), Times.Once());
        Assert.Equal(teamDto, result);
    }

    [Fact]
    public async Task AddMemberAsync_Should_AddMember_WhenTeamAndUserExist()
    {
        // Arrange
        var request = new AddMemberRequest(Guid.NewGuid(), Guid.NewGuid());
        var team = new Team { Id = request.TeamId, Name = "Test Team", TeamMembers = new List<TeamMember>() };

        _teamRepositoryMock.Setup(r => r.GetByIdAsync(request.TeamId)).ReturnsAsync(team);
        _userServiceClientMock.Setup(c => c.UserExistsAsync(request.UserId)).ReturnsAsync(true);

        // Act
        await _teamService.AddMemberAsync(request);

        // Assert
        _teamRepositoryMock.Verify(r => r.AddMemberAsync(It.IsAny<TeamMember>()), Times.Once());
    }

    [Fact]
    public async Task AddMemberAsync_Should_ThrowException_WhenTeamNotExists()
    {
        // Arrange
        var request = new AddMemberRequest(Guid.NewGuid(), Guid.NewGuid());
        _teamRepositoryMock.Setup(r => r.GetByIdAsync(request.TeamId)).ReturnsAsync((Team)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _teamService.AddMemberAsync(request));
    }
}