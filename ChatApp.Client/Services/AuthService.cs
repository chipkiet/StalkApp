using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatApp.Shared.DTOs.Auth;

namespace ChatApp.Client.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> RegisterAsync(RegisterRequest request)
    {
        return await _httpClient.PostAsJsonAsync("api/auth/register", request);
    }
}
