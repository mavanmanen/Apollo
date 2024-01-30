namespace Apollo.Handling.Services.WebhookBasicAuthentication;

public sealed class WebhookBasicAuthenticationConfiguration
{
    public string Realm { get; set; } = null!;
    public string Salt { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}