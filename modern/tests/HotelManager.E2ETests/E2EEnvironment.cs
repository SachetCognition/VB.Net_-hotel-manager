using System.Net.Http;

namespace HotelManager.E2ETests;

/// <summary>
/// Resolves the base URL under test and detects whether the app is actually
/// reachable. When it isn't, the Playwright flow test is skipped so that a
/// plain <c>dotnet test</c> (and the unit+component CI job) stays green without
/// a running server.
/// </summary>
public static class E2EEnvironment
{
    public const string DefaultBaseUrl = "http://localhost:8080";

    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("E2E_BASE_URL") is { Length: > 0 } url
            ? url.TrimEnd('/')
            : DefaultBaseUrl;

    /// <summary>Login credentials, overridable via env vars for CI.</summary>
    public static string UserName => Environment.GetEnvironmentVariable("E2E_USERNAME") ?? "admin";
    public static string Password => Environment.GetEnvironmentVariable("E2E_PASSWORD") ?? "admin@123";

    /// <summary>Returns true if the app answers an HTTP request within a short timeout.</summary>
    public static async Task<bool> IsAppReachableAsync()
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            using var response = await http.GetAsync(new Uri(BaseUrl + "/login"));
            return (int)response.StatusCode < 500;
        }
        catch
        {
            return false;
        }
    }
}
