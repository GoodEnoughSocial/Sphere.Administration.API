using IdentityModel.Client;
using System.Text.Json;

namespace Sphere.Administration.API.Tests;

public class UnitTest1
{
    [Fact]
    public async Task AuthTest()
    {
        // discover endpoints from metadata
        var client = new HttpClient();
        var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5004");
        if (disco.IsError)
        {
            Console.WriteLine(disco.Error);
            return;
        }

        // request token
        var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = disco.TokenEndpoint,

            ClientId = "client",
            ClientSecret = "secret",
            Scope = "api1"
        });

        if (tokenResponse.IsError)
        {
            Console.WriteLine(tokenResponse.Error);
            return;
        }

        Console.WriteLine(tokenResponse.AccessToken);

        // call api
        var apiClient = new HttpClient();
        apiClient.SetBearerToken(tokenResponse.AccessToken);

        var response = await apiClient.GetAsync("https://localhost:5002/WeatherForecast/test");
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.StatusCode);
        }
        else
        {
            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            var decoded = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(decoded);
        }
    }
}
