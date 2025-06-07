using System.Net.Http.Json;
using Application.DTOs;
using Application.Interfaces;

namespace Infrastructure.Services;

public class TeamServiceClient : ITeamServiceClient
{
    private readonly HttpClient _httpClient;

    public TeamServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> TeamExistsAsync(Guid teamId)
    {
        var response = await _httpClient.GetAsync($"api/teams/internal/exists/{teamId}");
        if (!response.IsSuccessStatusCode) return false;
        return await response.Content.ReadFromJsonAsync<bool>();
    }

    public async Task<bool> IsTeamMemberAsync(Guid teamId, Guid userId)
    {
        var response = await _httpClient.GetAsync($"api/teams/internal/{teamId}/members/{userId}");
        if (!response.IsSuccessStatusCode) return false;
        return await response.Content.ReadFromJsonAsync<bool>();
    }

    public async Task<IEnumerable<TeamDto>> GetUserTeamsAsync(Guid userId)
    {
        var response = await _httpClient.GetAsync($"api/teams/internal/user/{userId}");
        if (!response.IsSuccessStatusCode) return Enumerable.Empty<TeamDto>();
        return await response.Content.ReadFromJsonAsync<IEnumerable<TeamDto>>() ?? Enumerable.Empty<TeamDto>();
    }
}