namespace Apollo.Handling.Services.WebhookBasicAuthentication;

internal interface IWebhookBasicAuthenticationService
{
    public void InitUser();
    public bool ValidateUser(string username, string password);
}