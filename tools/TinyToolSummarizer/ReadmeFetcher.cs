using System.Text.Json;

namespace TinyToolSummarizer;

/// <summary>
/// Fetches README content from a GitHub repository.
/// </summary>
public static class ReadmeFetcher
{
    public static async Task<string?> FetchReadmeAsync(HttpClient httpClient, string githubUrl)
    {
        // Parse owner/repo from github URL
        // e.g. https://github.com/owner/repo or https://github.com/owner/repo/
        var uri = new Uri(githubUrl);
        var segments = uri.AbsolutePath.Trim('/').Split('/');
        if (segments.Length < 2)
            return null;

        var owner = segments[0];
        var repo = segments[1];

        // Try the GitHub API for the README
        var apiUrl = $"https://api.github.com/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/readme";

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Accept.ParseAdd("application/vnd.github.v3.raw");

            // Use GH_TOKEN if available for higher rate limits
            var token = Environment.GetEnvironmentVariable("GH_TOKEN");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var readme = await response.Content.ReadAsStringAsync();
                // Truncate very long READMEs to avoid token limits
                if (readme.Length > 4000)
                    readme = readme[..4000] + "\n\n[truncated]";
                return readme;
            }
        }
        catch
        {
            // Fall through
        }

        return null;
    }
}
