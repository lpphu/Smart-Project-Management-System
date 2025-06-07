using System.Net.Http.Json;
using Application.DTOs;
using Application.Interfaces;

namespace Infrastructure.Services;

public class ProjectServiceClient : IProjectServiceClient
{
    private readonly HttpClient _httpClient;

    public ProjectServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> ProjectExistsAsync(Guid projectId)
    {
        var response = await _httpClient.GetAsync($"api/projects/internal/exists/{projectId}");
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }
        return await response.Content.ReadFromJsonAsync<bool>();
    }

    public async Task<ProjectDto> GetProjectByIdAsync(Guid projectId)
    {
        var response = await _httpClient.GetAsync($"api/projects/internal/{projectId}");
        if (!response.IsSuccessStatusCode) throw new KeyNotFoundException("Project not found");
        return await response.Content.ReadFromJsonAsync<ProjectDto>() ??
               throw new KeyNotFoundException("Project not found");
    }
}