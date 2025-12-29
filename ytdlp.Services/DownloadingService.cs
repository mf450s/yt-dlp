using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ytdlp.Services.Interfaces;

namespace ytdlp.Services
{
    public class DownloadingService(
        IConfigsServices _configsServices,
        IProcessFactory? _processFactory = null
        ) : IDownloadingService
    {
        private readonly IConfigsServices configsService = _configsServices;
        private readonly IProcessFactory processFactory = _processFactory ?? new ProcessFactory();

        public async Task TryDownloadingFromURL(string url, string configFile, string? cookieFile = null)
        {
            string wholeConfigPath = configsService.GetWholeConfigPath(configFile);
            ProcessStartInfo startInfo = await GetProcessStartInfoAsync(url, wholeConfigPath, cookieFile);
            
            // Start the process
            using IProcess process = processFactory.CreateProcess();
            process.StartInfo = startInfo;
            process.Start();

            // Read the output and error streams
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            // Wait for the process to exit
            await process.WaitForExitAsync();

            // Log or handle the output and errors
            Console.WriteLine("Output:");
            Console.WriteLine(output);
            Console.WriteLine("Errors:");
            Console.WriteLine(error);
        }

        /// <summary>
        /// Creates and returns a <see cref="ProcessStartInfo"/> object with the necessary arguments to execute yt-dlp.
        /// </summary>
        /// <param name="url">The URL of the media to download.</param>
        /// <param name="wholeConfigPath">The path to the configuration file for yt-dlp.</param>
        /// <param name="wholeCookiePath">Optional: The path to the cookie file for authentication.</param>
        /// <returns>A <see cref="ProcessStartInfo"/> object configured to run yt-dlp with the provided URL and configuration.</returns>
        internal async Task<ProcessStartInfo> GetProcessStartInfoAsync(string url, string wholeConfigPath, string? wholeCookiePath = null)
        {
            // Construct the command and arguments for yt-dlp
            var args = new System.Collections.Generic.List<string>
            {
                url,
                "--config-locations",
                wholeConfigPath
            };

            // Add cookie file if provided
            if (!string.IsNullOrWhiteSpace(wholeCookiePath))
            {
                args.Add("--cookies-from-browser");
                args.Add(wholeCookiePath);
            }

            // Create a process start info object
            ProcessStartInfo startInfo = new()
            {
                FileName = "yt-dlp",
                Arguments = string.Join(" ", args),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            return await Task.FromResult(startInfo);
        }
    }
}
