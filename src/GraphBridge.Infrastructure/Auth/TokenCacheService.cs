using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using GraphBridge.Shared.Exceptions;

namespace GraphBridge.Infrastructure.Auth;

/// <summary>
/// Acquires and caches access tokens from Microsoft Entra ID using MSAL ConfidentialClientApplication.
/// Serves cached tokens until 5 minutes before their expiry time.
/// </summary>
public sealed class TokenCacheService : ITokenCacheService
{
    private readonly IConfidentialClientApplication _msalClient;
    private readonly string[] _scopes;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private string? _cachedToken;
    private DateTimeOffset _tokenExpiresOn;

    /// <summary>
    /// Buffer time before token expiry to trigger a refresh (5 minutes).
    /// </summary>
    private static readonly TimeSpan ExpiryBuffer = TimeSpan.FromMinutes(5);

    public TokenCacheService(IOptions<AzureAdOptions> options)
    {
        var config = options.Value;

        _scopes = config.Scopes;

        _msalClient = ConfidentialClientApplicationBuilder
            .Create(config.ClientId)
            .WithTenantId(config.TenantId)
            .WithClientSecret(config.ClientSecret)
            .Build();
    }

    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        // Return cached token if still valid (more than 5 minutes until expiry)
        if (!string.IsNullOrEmpty(_cachedToken) && DateTimeOffset.UtcNow < _tokenExpiresOn - ExpiryBuffer)
        {
            return _cachedToken;
        }

        await _semaphore.WaitAsync(ct);
        try
        {
            // Double-check after acquiring the lock
            if (!string.IsNullOrEmpty(_cachedToken) && DateTimeOffset.UtcNow < _tokenExpiresOn - ExpiryBuffer)
            {
                return _cachedToken;
            }

            var result = await _msalClient
                .AcquireTokenForClient(_scopes)
                .ExecuteAsync(ct);

            _cachedToken = result.AccessToken;
            _tokenExpiresOn = result.ExpiresOn;

            return _cachedToken;
        }
        catch (MsalException ex)
        {
            throw new AuthenticationException(
                $"Token acquisition failed: {ex.ErrorCode} - {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is not AuthenticationException)
        {
            throw new AuthenticationException(
                $"Token acquisition failed due to an unexpected error: {ex.Message}", ex);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
