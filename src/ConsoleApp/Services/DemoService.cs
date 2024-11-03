using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using ConsoleApp.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsoleApp.Services;

public interface IDemoService
{
    void WriteWelcomeMessage();
    Task<string> MakeGraphApiCall();
}

public class DemoService(ILogger<DemoService> logger,
        IClientCredentialService clientCredentialService,
        IOptions<ConsoleAppSettings> consoleAppSettings,
        IHttpClientFactory httpClientFactory) : IDemoService
{
    private readonly ILogger<DemoService> _logger = logger;
    private readonly IClientCredentialService _clientCredentialService = clientCredentialService;
    private readonly IOptions<ConsoleAppSettings> _consoleAppSettings = consoleAppSettings;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<string> MakeGraphApiCall()
    {
        var accessToken = await _clientCredentialService.GetAccessToken(
            ["https://graph.microsoft.com/.default"]);

        var httpClient = _httpClientFactory.CreateClient("GraphApi");
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetAsync("/v1.0/users");

        JsonNode? result = null;
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            result = JsonNode.Parse(json);
        }
        return result is null
            ? ""
            : result.ToJsonString();
    }

    public void WriteWelcomeMessage()
    {
        _logger.LogDebug("Saying hello");
        _logger.LogInformation(_consoleAppSettings.Value?.WelcomeMessage);
    }
}
