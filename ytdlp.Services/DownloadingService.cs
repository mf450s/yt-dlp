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
        public async Task TryDownloadingFromURL(string url, string configFile)
        {
            string wholeConfigPath = configsService.GetWholeConfigPath(configFile);
            ProcessStartInfo startInfo = await GetProcessStartInfoAsync(url, wholeConfigPath);
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
        internal async Task<ProcessStartInfo> GetProcessStartInfoAsync(string url, string wholeConfigPath)
        {
            // Construct the command and arguments for yt-dlp
            string[] args =
            [
                url,
                $"--config-locations", wholeConfigPath
            ];

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
            return startInfo;
        }
    }
}
