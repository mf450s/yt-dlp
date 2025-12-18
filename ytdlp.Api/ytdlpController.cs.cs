using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ytdlp.Services.Interfaces;

namespace ytdlp.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ytdlpController(IDownloadingService downloadingService) : ControllerBase
    {
        [HttpPost("download")]
        public IActionResult Download([FromBody] string url, [FromQuery] string configfilename)
        {
            downloadingService.TryDownloadingFromURL(url, configfilename);
            return Accepted();
        }
    }
}
