using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace ConsoleApp.Services;

public interface IClientCredentialService
{
    Task<AuthenticationResult?> GetAuthenticationResult(IEnumerable<string>? scopes = null);
    Task<string> GetAccessToken(IEnumerable<string>? scopes = null);
}

public class ClientCredentialService(ILogger<ClientCredentialService> logger,
        IOptions<ConfidentialClientApplicationOptions> confidentialClientApplicationOptions) : IClientCredentialService
{
    private readonly ILogger<ClientCredentialService> _logger = logger;
    private readonly IConfidentialClientApplication? _confidentialClientApplication = 
        confidentialClientApplicationOptions?.Value?.ClientId is not null
            ? ConfidentialClientApplicationBuilder
                .CreateWithApplicationOptions(confidentialClientApplicationOptions.Value)
                .Build()
            : null;

    public async Task<string> GetAccessToken(IEnumerable<string>? scopes = null)
    {
        var result = await GetAuthenticationResult(scopes);
        if (result is null)
        {
            throw new Exception("Failed to acquire token");
        }
        return result.AccessToken;
    }

    public async Task<AuthenticationResult?> GetAuthenticationResult(IEnumerable<string>? scopes = null)
    {
        if (_confidentialClientApplication is null)
        {
            return null;
        }
        AuthenticationResult? result = null;
        scopes ??= [".default"];
        try
        {
            _logger.LogDebug($"Acquiring token with scopes: '{string.Join(" ", scopes)}'");
            result = await _confidentialClientApplication
                .AcquireTokenForClient(scopes)
                .ExecuteAsync();
            _logger.LogDebug("Acquired token");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to acquire token");
            throw;
        }
        return result;
    }
}
