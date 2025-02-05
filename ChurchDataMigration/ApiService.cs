using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChurchData;
using ChurchData.DTOs;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task ImportDataAsync<T>(List<T> data, string apiUrl)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(apiUrl, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error: {response.StatusCode}, Content: {responseContent}");
        }

        response.EnsureSuccessStatusCode();
    }   
    public void ImportData<T>(T data, string apiUrl)
    {
        try
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync(apiUrl, jsonContent).Result;

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"Error: {response.StatusCode}, Content: {responseContent}");
            }

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            // Log the detailed error message
            Console.WriteLine($"Request error: {ex.Message}");
            // Handle or rethrow the exception as needed
            throw;
        }
        catch (Exception ex)
        {
            // Log any other exceptions that might occur
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetUnitNamesAsync(string apiUrl)
    {
        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var units = JsonSerializer.Deserialize<List<Unit>>(jsonResponse, options);

        var unitNames = new Dictionary<string, int>();
        foreach (var unit in units)
        {
            unitNames[unit.UnitName] = unit.UnitId;
        }

        return unitNames;
    }
}
