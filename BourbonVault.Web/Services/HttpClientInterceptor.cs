using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BourbonVault.Web.Services
{
    public class HttpClientInterceptor
    {
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _navigationManager;

        public HttpClientInterceptor(ILocalStorageService localStorage, NavigationManager navigationManager)
        {
            _localStorage = localStorage;
            _navigationManager = navigationManager;
        }

        public void RegisterEvent()
        {
            // Register the BeforeSendEvent to intercept requests
            AppDomain.CurrentDomain.ProcessExit += (_, _) => UnregisterEvent();
            HttpRequestInterceptor.ProcessBeforeSendEvent += ProcessBeforeSendAsync;
        }

        public void UnregisterEvent()
        {
            HttpRequestInterceptor.ProcessBeforeSendEvent -= ProcessBeforeSendAsync;
        }

        private async Task ProcessBeforeSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if we have a stored token
                var token = await _localStorage.GetItemAsync<string>("authToken");
                
                // If we have a token, add it to the Authorization header
                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HTTP interceptor: {ex.Message}");
            }
        }
    }

    // Helper to raise an event before sending requests
    public static class HttpRequestInterceptor
    {
        public static event Func<HttpRequestMessage, CancellationToken, Task> ProcessBeforeSendEvent;

        public static async Task ProcessBeforeSend(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ProcessBeforeSendEvent != null)
            {
                await ProcessBeforeSendEvent(request, cancellationToken);
            }
        }
    }

    // Extension class for HttpClient to use our custom handler
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> SendWithInterceptorAsync(
            this HttpClient client,
            HttpRequestMessage request,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            await HttpRequestInterceptor.ProcessBeforeSend(request, cancellationToken);
            return await client.SendAsync(request, completionOption, cancellationToken);
        }

        public static async Task<HttpResponseMessage> SendWithInterceptorAsync(
            this HttpClient client,
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await HttpRequestInterceptor.ProcessBeforeSend(request, cancellationToken);
            return await client.SendAsync(request, cancellationToken);
        }
    }
}
