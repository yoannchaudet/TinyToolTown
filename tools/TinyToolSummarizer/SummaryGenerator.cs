using GitHub.Copilot.SDK;

namespace TinyToolSummarizer;

/// <summary>
/// Uses the Copilot SDK to generate fun AI summaries of tools.
/// </summary>
public static class SummaryGenerator
{
    public static async Task<string?> GenerateSummaryAsync(
        CopilotSession session, string toolName, string tagline, string readmeContent)
    {
        var prompt = $"""
            You are a witty, fun tech writer for "Tiny Tool Town" — a curated collection of small, 
            delightful open source tools. Write a brief AI summary for the following tool.
            
            Start with a fun emoji bullet point that captures the vibe of the tool (e.g. 🎮 for games, 
            🔒 for security, 🎨 for design, ⚡ for performance, 🛠️ for dev tools, etc.). Then write 
            1-2 playful, enthusiastic sentences about what makes it cool or unique.
            
            Keep it under 200 characters total. Do NOT use quotes or special YAML characters like colons 
            in your response. Do NOT include the tool name at the start. Just the emoji and summary text, 
            nothing else.
            
            Tool: {toolName}
            Tagline: {tagline}
            
            README content:
            {readmeContent}
            """;

        var result = new System.Text.StringBuilder();
        var done = new TaskCompletionSource();

        var subscription = session.On(evt =>
        {
            switch (evt)
            {
                case AssistantMessageDeltaEvent delta:
                    result.Append(delta.Data.DeltaContent);
                    break;
                case AssistantMessageEvent msg:
                    if (result.Length == 0)
                        result.Append(msg.Data.Content);
                    break;
                case SessionIdleEvent:
                    done.TrySetResult();
                    break;
                case SessionErrorEvent err:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($" [AI error: {err.Data.Message}] ");
                    Console.ResetColor();
                    done.TrySetResult();
                    break;
            }
        });

        try
        {
            await session.SendAsync(new MessageOptions { Prompt = prompt });
            await done.Task;
        }
        finally
        {
            subscription.Dispose();
        }

        var summary = result.ToString().Trim().Trim('"');
        // Clean up any YAML-unsafe characters
        summary = summary.Replace("\n", " ").Replace("\r", "").Trim();
        return string.IsNullOrWhiteSpace(summary) ? null : summary;
    }
}
