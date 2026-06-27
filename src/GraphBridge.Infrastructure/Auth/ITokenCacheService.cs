namespace GraphBridge.Infrastructure.Auth;

/// <summary>
/// Provides access token acquisition and caching for Microsoft Graph API calls in Live_Mode.
/// </summary>
public interface ITokenCacheService
{
    /// <summary>
    /// Gets a valid access token for Microsoft Graph API.
    /// Returns a cached token if it has more than 5 minutes remaining before expiry;
    /// otherwise acquires a new token from Microsoft Entra ID via MSAL.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A valid access token string.</returns>
    /// <exception cref="GraphBridge.Shared.Exceptions.AuthenticationException">
    /// Thrown when token acquisition fails due to network error, invalid credentials, or configuration issues.
    /// </exception>
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
}
