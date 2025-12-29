namespace ytdlp.Services.Interfaces;

public interface IDownloadingService
{
    /// <summary>
    /// Attempts to download content from a URL using the specified configuration and optional cookie file.
    /// </summary>
    /// <param name="url">The URL to download from.</param>
    /// <param name="configFile">The name of the configuration file to use.</param>
    /// <param name="cookieFile">Optional: The name of the cookie file to use for authentication.</param>
    /// <returns>A task representing the asynchronous download operation.</returns>
    Task TryDownloadingFromURL(string url, string configFile, string? cookieFile = null);
}
