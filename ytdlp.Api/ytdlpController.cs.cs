using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ytdlp.Services;
using ytdlp.Services.Interfaces;

namespace ytdlp.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ytdlpController(IDownloadingService downloadingService, IConfigsServices configsServices) : ControllerBase
    {
        [HttpPost("download")]
        public IActionResult Download([FromBody] string url, [FromQuery] string configfilename)
        {
            downloadingService.TryDownloadingFromURL(url, configfilename);
            return Accepted();
        }
        [HttpGet("config")]
        public List<string> GetConfigFileNames()
        {
            return configsServices.GetAllConfigNames();
        }
        [HttpGet("config/{configName}")]
        public IActionResult GetConfigByName(string configName)
        {
            string configContent = configsServices.GetConfigContent(configName);
            if (string.IsNullOrEmpty(configContent))
            {
                return NotFound($"Configuration file '{configName}' not found.");
            }
            return Ok(configContent);
        }
    }
}
