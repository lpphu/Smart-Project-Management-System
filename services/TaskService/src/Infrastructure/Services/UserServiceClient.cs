using System.Net.Http.Json;
using Application.DTOs;
using Application.Interfaces;

namespace Infrastructure.Services;

public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;

    public UserServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var response = await _httpClient.GetAsync($"api/users/internal/{userId}");
        if (!response.IsSuccessStatusCode) throw new KeyNotFoundException("User not found");
        return await response.Content.ReadFromJsonAsync<UserDto>() ?? throw new KeyNotFoundException("User not found");
    }

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        var response = await _httpClient.GetAsync($"api/users/internal/exists/{userId}");
        if (!response.IsSuccessStatusCode) return false;
        return await response.Content.ReadFromJsonAsync<bool>();
    }
}