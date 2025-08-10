using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BourbonVault.Web;
using BourbonVault.Web.Services;
using System.Net.Http.Headers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for the API
builder.Services.AddScoped(sp => 
{
    var httpClient = new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiUrl"] ?? builder.HostEnvironment.BaseAddress) };
    return httpClient;
});

// Add Blazored LocalStorage for storing JWT token
builder.Services.AddBlazoredLocalStorage();

// Add Authentication services
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();

// Register custom services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBottleService, BottleService>();
builder.Services.AddScoped<IVisionService, VisionService>();
builder.Services.AddScoped<ITastingNoteService, TastingNoteService>();

// Register HttpClientInterceptor to add JWT token to requests
builder.Services.AddScoped<HttpClientInterceptor>();
builder.Services.AddScoped(sp => 
{
    var interceptor = sp.GetRequiredService<HttpClientInterceptor>();
    var httpClient = new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiUrl"] ?? builder.HostEnvironment.BaseAddress) };
    
    // Register interceptor for attaching JWT token
    interceptor.RegisterEvent(); 
    
    return httpClient;
});

await builder.Build().RunAsync();
