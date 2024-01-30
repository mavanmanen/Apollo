namespace Apollo.Handling.Services.WebhookBasicAuthentication;

public sealed class WebhookBasicAuthenticationConfiguration
{
    public string Realm { get; set; }
    public string Salt { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}