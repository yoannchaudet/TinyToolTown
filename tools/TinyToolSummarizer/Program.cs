using GitHub.Copilot.SDK;
using TinyToolSummarizer;

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║       Tiny Tool Town — AI Summary Generator 🏘️✨          ║");
Console.WriteLine("║       Powered by GitHub Copilot SDK                        ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.ResetColor();
Console.WriteLine();

// Resolve the content/tools directory
var contentDir = args.Length > 0
    ? args[0]
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "content", "tools"));

if (!Directory.Exists(contentDir))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ Tools directory not found: {contentDir}");
    Console.WriteLine("   Usage: dotnet run [path-to-src/content/tools]");
    Console.ResetColor();
    return;
}

Console.WriteLine($"📂 Tools directory: {contentDir}");

var mdFiles = Directory.GetFiles(contentDir, "*.md");
Console.WriteLine($"📋 Found {mdFiles.Length} tool files\n");

// Check prerequisites
Console.WriteLine("🔍 Checking Copilot prerequisites...");
CopilotClient? client = null;
CopilotSession? session = null;

try
{
    client = new CopilotClient();
    await client.StartAsync();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("✅ Copilot client started\n");
    Console.ResetColor();

    session = await client.CreateSessionAsync(new SessionConfig
    {
        Model = "gpt-4o",
        Streaming = true
    });
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"✅ Session created (ID: {session.SessionId})\n");
    Console.ResetColor();

    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TinyToolSummarizer/1.0");

    var processed = 0;
    var skipped = 0;
    var failed = 0;

    foreach (var mdFile in mdFiles)
    {
        var fileName = Path.GetFileName(mdFile);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"[{processed + skipped + failed + 1}/{mdFiles.Length}] ");
        Console.ResetColor();
        Console.Write($"{fileName} ");

        try
        {
            var content = await File.ReadAllTextAsync(mdFile);
            var parsed = FrontmatterParser.Parse(content);

            // Skip if already has an AI summary
            if (parsed.Frontmatter.ContainsKey("ai_summary"))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⏭️  already has ai_summary");
                Console.ResetColor();
                skipped++;
                continue;
            }

            if (!parsed.Frontmatter.TryGetValue("github_url", out var githubUrl))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠️  no github_url");
                Console.ResetColor();
                failed++;
                continue;
            }

            var toolName = parsed.Frontmatter.GetValueOrDefault("name", fileName);
            var tagline = parsed.Frontmatter.GetValueOrDefault("tagline", "");

            // Fetch the README from GitHub
            var readme = await ReadmeFetcher.FetchReadmeAsync(httpClient, githubUrl.Trim('"'));

            if (string.IsNullOrWhiteSpace(readme))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⚠️  no README found, using existing content");
                Console.ResetColor();
                // Fall back to the body content if available
                readme = parsed.Body;
            }

            if (string.IsNullOrWhiteSpace(readme) && string.IsNullOrWhiteSpace(tagline))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠️  no content to summarize");
                Console.ResetColor();
                failed++;
                continue;
            }

            // Generate AI summary
            var summary = await SummaryGenerator.GenerateSummaryAsync(
                session, toolName.Trim('"'), tagline.Trim('"'), readme ?? "");

            if (string.IsNullOrWhiteSpace(summary))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ empty summary");
                Console.ResetColor();
                failed++;
                continue;
            }

            // Write the ai_summary back into the frontmatter
            var updatedContent = FrontmatterParser.AddField(content, "ai_summary", summary);
            await File.WriteAllTextAsync(mdFile, updatedContent);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ \"{summary[..Math.Min(60, summary.Length)]}...\"");
            Console.ResetColor();
            processed++;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ {ex.Message}");
            Console.ResetColor();
            failed++;
        }
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("═══════════════════════════════════════════════");
    Console.WriteLine($"  ✅ Processed: {processed}");
    Console.WriteLine($"  ⏭️  Skipped:   {skipped}");
    Console.WriteLine($"  ❌ Failed:    {failed}");
    Console.WriteLine("═══════════════════════════════════════════════");
    Console.ResetColor();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n❌ Fatal error: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"   Inner: {ex.InnerException.Message}");
    Console.ResetColor();
}
finally
{
    if (session != null) await session.DisposeAsync();
    if (client != null) await client.DisposeAsync();
    Console.WriteLine("\n🛑 Done.");
}
