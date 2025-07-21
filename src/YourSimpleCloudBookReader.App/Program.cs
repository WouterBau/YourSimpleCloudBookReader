using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using YourSimpleCloudBookReader.App;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//Specific for WebAssembly, can't read as string[] from appsettings.json
var scopes = builder.Configuration.GetValue<string>("MicrosoftGraph:Scopes").Split(';');

builder.Services.AddMicrosoftGraphClient(scopes);

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    foreach (var scope in scopes)
    {
        options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);
    }
});

await builder.Build().RunAsync();
