using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;

namespace Apollo.Handling;

public abstract class ApiClient
{
    private readonly RestClient _client;

    protected ApiClient(string baseUrl,
        IAuthenticator? authenticator = null,
        JsonSerializerSettings? serializerSettings = null,
        Dictionary<string, string>? defaultHeaders = null)
    {
        _client = new RestClient(
            baseUrl,
            configureRestClient: cfg =>
            {
                cfg.Authenticator = authenticator;
            },
            configureSerialization: cfg =>
            {
                if (serializerSettings is null)
                {
                    cfg.UseNewtonsoftJson();
                }
                else
                {
                    cfg.UseNewtonsoftJson(serializerSettings);
                }
            });
        
        if (defaultHeaders is not null)
        {
            _client.AddDefaultHeaders(defaultHeaders);
        }
    }

    protected virtual async Task<TResult?> DoRequestAsync<TResult>(Method method,
        string resource,
        IEnumerable<Parameter>? parameters = null,
        object? body = null)
        where TResult : class
    {
        var request = CreateRequest(method, resource, parameters, body);
        var result = await _client.ExecuteAsync<TResult>(request);

        return result.Data;
    }

    private async Task DoRequestAsync(Method method, string resource, IEnumerable<Parameter>? parameters = null,
        object? body = null)
    {
        var request = CreateRequest(method, resource, parameters, body);
        var result = await _client.ExecuteAsync(request);
    }

    public Task<TResult?> GetAsync<TResult>(string resource, IEnumerable<Parameter>? parameters = null) where TResult : class =>
        DoRequestAsync<TResult>(Method.Get, resource, parameters);

    public Task<TResult?> PostAsync<TResult>(string resource, IEnumerable<Parameter>? parameters = null, object? body = null) where TResult : class =>
        DoRequestAsync<TResult>(Method.Post, resource, parameters, body);

    public Task<TResult?> PutAsync<TResult>(string resource, IEnumerable<Parameter>? parameters = null, object? body = null) where TResult : class =>
        DoRequestAsync<TResult>(Method.Put, resource, parameters, body);

    public Task DeleteAsync(string resource, IEnumerable<Parameter>? parameters = null) =>
        DoRequestAsync(Method.Delete, resource, parameters);

    private static RestRequest CreateRequest(Method method, string resource, IEnumerable<Parameter>? parameters = null, object? body = null)
    {
        var request = new RestRequest(resource, method);

        if (parameters is not null)
        {
            foreach (var parameter in parameters)
            {
                request.AddParameter(parameter);
            }
        }

        if (body is not null)
        {
            request.AddBody(body);
        }

        return request;
    }
}