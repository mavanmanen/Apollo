using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Apollo.Handling.Services.WebhookBasicAuthentication;

internal class WebhookBasicAuthenticationService(IOptions<WebhookBasicAuthenticationConfiguration> options) : IWebhookBasicAuthenticationService
{
    private string _username = null!;
    private string _salt = null!;
    private string _hashedPassword = null!;
    
    public void InitUser()
    {
        _username = options.Value.Username;
        _salt = options.Value.Salt;
        _hashedPassword = GetHashedPassword(options.Value.Password);
    }
    
    public bool ValidateUser(string username, string password)
    {
        return username.Equals(_username, StringComparison.OrdinalIgnoreCase) && CompareHash(password);
    }

    private bool CompareHash(string attempt) =>
        GetHashedPassword(attempt).Equals(_hashedPassword);

    private string GetHashedPassword(string password) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.Unicode.GetBytes(string.Concat(password, _salt))));
}