namespace GraphBridge.Infrastructure.Auth;

/// <summary>
/// Configuration POCO bound to the "GraphBridge:AzureAd" configuration section.
/// Contains Microsoft Entra ID settings for MSAL token acquisition.
/// </summary>
public class AzureAdOptions
{
    public const string SectionName = "GraphBridge:AzureAd";

    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = new[] { "https://graph.microsoft.com/.default" };
}
