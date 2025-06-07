using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace UnitTests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _passwordHasherMock = new Mock<IPasswordHasher<User>>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _userService = new UserService(
            _userRepositoryMock.Object,
            _mapperMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_Should_CreateUser_And_ReturnUserDto()
    {
        var request = new RegisterRequest("testuser", "test@example.com", "password123");
        var user = new User { Id = Guid.NewGuid(), Username = request.Username, Email = request.Email };
        var userDto = new UserDto { Id = user.Id, Username = request.Username, Email = request.Email };
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email))!.ReturnsAsync((User)null!);
        _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<User>(), request.Password)).Returns("hashed_password");
        _mapperMock.Setup(m => m.Map<UserDto>(It.IsAny<User>())).Returns(userDto);
        var result = await _userService.RegisterAsync(request);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once());
        Assert.Equal(userDto, result);
    }

    [Fact]
    public async Task LoginAsync_Should_ThrowException_WhenCredentialsInvalid()
    {
        var request = new LoginRequest("test@example.com", "wrongpassword");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email))!.ReturnsAsync((User)null!);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.LoginAsync(request));
    }
}