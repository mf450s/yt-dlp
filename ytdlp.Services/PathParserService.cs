using Microsoft.Extensions.Options;
using ytdlp.Configs;
using ytdlp.Services.Interfaces;

namespace ytdlp.Services;

public class PathParserService(IOptions<PathConfiguration> paths) : IPathParserService
{
    private readonly string configFolder = paths.Value.Config;
    private readonly string downloadFolder = paths.Value.Downloads;
    private readonly string archiveFolder = paths.Value.Archive;
    public string CheckAndFixPaths(string line)
    {
        string trimmed = line.Trim();

        // Check for -o or --output
        if (trimmed.StartsWith("-o ") || trimmed.StartsWith("--output "))
        {
            return FixPath(trimmed, downloadFolder);
        }

        if (trimmed.StartsWith("--download-archive"))
        {
            return FixPath(trimmed, archiveFolder);
        }
        return line;
    }
    internal string FixPath(string line, string folder)
    {
        string[] parts = line.Split([' '], 2);

        if (parts.Length != 2)
            return line;

        string template = parts[1].Trim();

        // Remove quotes if present
        if (template.StartsWith("\"") && template.EndsWith("\""))
        {
            template = template.Substring(1, template.Length - 2);
            template = template.Trim();
        }

        // Add folder if not already present
        if (!template.Contains(folder))
        {
            // Remove leading "/" to avoid "//"
            if (template.StartsWith("/"))
                template = template[1..];
            template = $"{folder}{template}";
        }
        return $"{parts[0]} \"{template}\"";
    }
}
