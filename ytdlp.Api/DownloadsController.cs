using Microsoft.AspNetCore.Mvc;
using ytdlp.Services.Interfaces;

namespace ytdlp.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadsController(
        IConfigsServices configsServices,
        ILogger<DownloadsController> logger,
        IServiceScopeFactory scopeFactory
        ) : ControllerBase
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<DownloadsController> _logger = logger;

        /// <summary>
        /// Downloads content from a URL using a specified configuration.
        /// Cookie files should be specified within the config file using --cookies option.
        /// </summary>
        /// <param name="url">The URL to download from.</param>
        /// <param name="confName">The name of the configuration file to use.</param>
        [HttpPost("download")]
        public async Task<IActionResult> Download([FromBody] string url, [FromQuery] string confName)
        {
            var correlationId = HttpContext.TraceIdentifier;
            _logger.LogInformation(
                "[{CorrelationId}] ⬇️ Download request received | URL: {Url} | Config: {ConfigName}",
                correlationId, url, confName);

            // Validate configuration file exists
            var configResult = configsServices.GetConfigContentByName(confName);
            if (configResult.IsFailed)
            {
                _logger.LogWarning(
                    "[{CorrelationId}] ⚠️ Download validation failed | Config '{ConfigName}' not found",
                    correlationId, confName);
                return BadRequest(new { error = $"Configuration '{confName}' not found.", correlationId });
            }

            // Fire-and-Forget
            _ = Task.Run(async () =>
            {
                // new scope for background work
                using var scope = _scopeFactory.CreateScope();
                try
                {
                    var backgroundDownloadingService = scope.ServiceProvider.GetRequiredService<IDownloadingService>();

                    _logger.LogInformation(
                        "[{CorrelationId}] ▶️ Background download starting | URL: {Url}",
                        correlationId, url);

                    await backgroundDownloadingService.TryDownloadingFromURL(url, confName);

                    _logger.LogInformation(
                        "[{CorrelationId}] ✅ Background download finished | URL: {Url}",
                        correlationId, url);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "[{CorrelationId}] 🚨 Error during background download | URL: {Url} | Config: {ConfigName}",
                        correlationId, url, confName);
                }
            });

            // Immediate accepted response
            _logger.LogInformation(
                "[{CorrelationId}] 🚀 Download accepted and queued (processing in background)",
                correlationId);

            return Accepted(new { message = "Download started in background", url, config = confName, correlationId });
        }
    }
}
