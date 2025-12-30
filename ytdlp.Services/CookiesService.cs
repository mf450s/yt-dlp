using FluentResults;
using Microsoft.Extensions.Options;
using ytdlp.Configs;
using ytdlp.Services.Interfaces;

namespace ytdlp.Services
{
    /// <summary>
    /// Service for managing cookie files used by yt-dlp.
    /// Supports Netscape format and JSON-based cookie files.
    /// </summary>
    public class CookiesService(IPathParserService pathParserService, IOptions<PathConfiguration> paths) : ICookiesService
    {
        private readonly IPathParserService _pathParserService = pathParserService ?? throw new ArgumentNullException(nameof(pathParserService));
        private readonly string cookiePath = paths.Value.Cookies;

        /// <summary>
        /// Retrieves all available cookie file names from the cookies directory.
        /// </summary>
        public List<string> GetAllCookieNames()
        {
            try
            {
                if (!Directory.Exists(cookiePath))
                    return [];

                return Directory.GetFiles(cookiePath).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving cookie names: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Retrieves the content of a specific cookie file.
        /// </summary>
        public Result<string> GetCookieContentByName(string cookieName)
        {
            if (string.IsNullOrWhiteSpace(cookieName))
                return Result.Fail("Cookie name cannot be empty.");

            try
            {
                string wholePath = GetWholeCookiePath(cookieName);

                if (!File.Exists(wholePath))
                    return Result.Fail($"Cookie file '{cookieName}' not found.");

                string content = File.ReadAllText(wholePath);
                return Result.Ok(content);
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error reading cookie file: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a cookie file by name.
        /// </summary>
        public Result<string> DeleteCookieByName(string cookieName)
        {
            if (string.IsNullOrWhiteSpace(cookieName))
                return Result.Fail("Cookie name cannot be empty.");

            try
            {
                string wholePath = GetWholeCookiePath(cookieName);

                if (!File.Exists(wholePath))
                    return Result.Fail($"Cookie file '{cookieName}' not found.");

                File.Delete(wholePath);
                return Result.Ok($"Cookie file '{cookieName}' deleted successfully.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error deleting cookie file: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new cookie file with the provided content.
        /// </summary>
        public async Task<Result<string>> CreateNewCookieAsync(string cookieName, string cookieContent)
        {
            if (string.IsNullOrWhiteSpace(cookieName))
                return Result.Fail("Cookie name cannot be empty.");

            if (string.IsNullOrWhiteSpace(cookieContent))
                return Result.Fail("Cookie content cannot be empty.");

            try
            {
                string wholePath = GetWholeCookiePath(cookieName);

                // Ensure directory exists
                string directory = Path.GetDirectoryName(wholePath)!;
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // Check if file already exists
                if (File.Exists(wholePath))
                    return Result.Fail($"Cookie file '{cookieName}' already exists.");

                // Validate cookie content format (basic validation)
                if (!IsValidCookieFormat(cookieContent))
                    return Result.Fail("Invalid cookie file format. Expected Netscape format or valid JSON.");

                await File.WriteAllTextAsync(wholePath, cookieContent);
                return Result.Ok($"Cookie file '{cookieName}' created successfully.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error creating cookie file: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the content of an existing cookie file.
        /// </summary>
        public async Task<Result<string>> SetCookieContentAsync(string cookieName, string cookieContent)
        {
            if (string.IsNullOrWhiteSpace(cookieName))
                return Result.Fail("Cookie name cannot be empty.");

            if (string.IsNullOrWhiteSpace(cookieContent))
                return Result.Fail("Cookie content cannot be empty.");

            try
            {
                string wholePath = GetWholeCookiePath(cookieName);

                if (!File.Exists(wholePath))
                    return Result.Fail($"Cookie file '{cookieName}' not found.");

                // Validate cookie content format
                if (!IsValidCookieFormat(cookieContent))
                    return Result.Fail("Invalid cookie file format. Expected Netscape format or valid JSON.");

                await File.WriteAllTextAsync(wholePath, cookieContent);
                return Result.Ok($"Cookie file '{cookieName}' updated successfully.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error updating cookie file: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the full file path for a cookie file.
        /// </summary>
        public string GetWholeCookiePath(string cookieName)
        {
            return Path.Combine(cookiePath, cookieName);
        }

        /// <summary>
        /// Basic validation for cookie file format.
        /// Supports Netscape format (tab-separated) and JSON format.
        /// </summary>
        private static bool IsValidCookieFormat(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            // Allow Netscape format comments
            if (content.StartsWith("#") || content.StartsWith("// Netscape HTTP Cookie File"))
                return true;

            // Allow JSON format
            if (content.TrimStart().StartsWith("{") || content.TrimStart().StartsWith("["))
                return true;

            // Allow tab-separated Netscape format
            var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0 && lines.All(line => line.StartsWith("#") || ContainsTabSeparatedCookieFormat(line)))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if a line follows the Netscape cookie format (tab-separated values).
        /// </summary>
        private static bool ContainsTabSeparatedCookieFormat(string line)
        {
            // Netscape format: domain, flag, path, secure, expiration, name, value
            var parts = line.Split('\t');
            return parts.Length == 7 || line.StartsWith("#");
        }
    }
}
