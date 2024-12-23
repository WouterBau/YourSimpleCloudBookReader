using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Authentication.WebAssembly.Msal.Models;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using IAccessTokenProvider = Microsoft.AspNetCore.Components.WebAssembly.Authentication.IAccessTokenProvider;

/// <summary>
/// Adds services and implements methods to use Microsoft Graph SDK.
/// </summary>
internal static class GraphClientExtensions
{
    /// <summary>
    /// Extension method for adding the Microsoft Graph SDK to IServiceCollection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="scopes">The MS Graph scopes to request</param>
    /// <returns></returns>
    public static IServiceCollection AddMicrosoftGraphClient(this IServiceCollection services, params string[] scopes)
    {
        services.Configure<RemoteAuthenticationOptions<MsalProviderOptions>>(options =>
        {
            foreach (var scope in scopes)
            {
                options.ProviderOptions.AdditionalScopesToConsent.Add(scope);
            }
        });

        services.AddScoped<IAuthenticationProvider, GraphAuthenticationProvider>();
        // Specific for WebAssembly, can't create GrahpServiceClient without providing a HttpClient
        services.AddHttpClient<GraphServiceClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(300);
        }).AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
        services.AddScoped(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(GraphServiceClient));
            return new GraphServiceClient(
                httpClient,
                sp.GetRequiredService<IAuthenticationProvider>());
        });
        return services;
    }


    /// <summary>
    /// Implements IAuthenticationProvider interface.
    /// Tries to get an access token for Microsoft Graph.
    /// </summary>
    /// <param name="Provider"></param>
    private record GraphAuthenticationProvider(IAccessTokenProvider Provider) : IAuthenticationProvider
    {
        // Implementation of IAuthenticationProvider was wrong in template
        public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            var result = await Provider.RequestAccessToken(new AccessTokenRequestOptions()
            {
                Scopes = ["https://graph.microsoft.com/User.Read"]
            });

            if (result.TryGetToken(out var token))
            {
                request.Headers.Add("Authorization", $"{CoreConstants.Headers.Bearer} {token.Value}");
            }
        }
    }
}
