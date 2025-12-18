using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ytdlp.Services.Interfaces;

namespace ytdlp.Services
{
    public class DownloadingService : IDownloadingService
    {
        public async Task TryDownloadingFromURL(string url, string configFile)
        {
            string wholeConfigPath = GetWholeConfigPath(configFile);
            ProcessStartInfo startInfo = await GetProcessStartInfoAsync(url, wholeConfigPath);
            // Start the process
            using Process process = new() { StartInfo = startInfo };
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
        private async Task<ProcessStartInfo> GetProcessStartInfoAsync(string url, string wholeConfigPath)
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
        private string GetWholeConfigPath(string configName)
        {
            return $"../configs/{configName}.conf";
        }
    }
}
