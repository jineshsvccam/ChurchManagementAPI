using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AccessDataMigration;
using ChurchData;
using ChurchDTOs.DTOs.Entities;

public class ApiService
{
    private readonly HttpClient _httpClient;
    
    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void SetAuthorizationToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
    public async Task ImportData<T>(T data, string apiUrl)
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

    public async Task ImportUnits(List<UnitDto> units, string apiUrl)
    {
        var apiService = new ApiService(_httpClient);

        foreach (var unit in units)
        {
            try
            {
                apiService.ImportData(unit, apiUrl);
            }
            catch (HttpRequestException ex)
            {
                // Log the detailed error message
                Console.WriteLine($"Request error for Unit: {unit.UnitName}, Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log any other exceptions that might occur
                Console.WriteLine($"Unexpected error for Unit: {unit.UnitName}, Error: {ex.Message}");
            }
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
    public async Task<Dictionary<string, int>> GetFamiliesAsync(string apiUrl)
    {
        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var families = JsonSerializer.Deserialize<List<Family>>(jsonResponse, options);

        var familyNames = new Dictionary<string, int>();
        foreach (var family in families)
        {
            familyNames[family.FamilyNumber.ToString()] = family.FamilyId;
        }

        return familyNames;
    }
    public async Task<Dictionary<string, int>> GetBanksAsync(string apiUrl)
    {
        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var banks = JsonSerializer.Deserialize<List<Bank>>(jsonResponse, options);

        var bankNames = new Dictionary<string, int>();
        foreach (var bank in banks)
        {
            bankNames[bank.BankName] = bank.BankId;
        }

        return bankNames;
    }
    public async Task<Dictionary<string, int>> GetHeadsNamesAsync(string apiUrl)
    {
        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var heads = JsonSerializer.Deserialize<List<TransactionHead>>(jsonResponse, options);

        var headNames = new Dictionary<string, int>();
        foreach (var head in heads)
        {
            headNames[head.HeadName] = head.HeadId;
        }

        return headNames;
    }
    public async Task AuthenticateAsync(string authUrl, string username, string password)
    {
        var loginData = new { username, password };
        var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(authUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (authResponse != null && !string.IsNullOrEmpty(authResponse.Token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);
                Console.WriteLine("Authentication successful, token assigned.");
            }
        }
        else
        {
            Console.WriteLine($"Authentication failed: {response.StatusCode}");
        }
    }


}
