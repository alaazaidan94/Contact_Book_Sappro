using ContactBook_Services;
using Microsoft.AspNetCore.Mvc;

namespace ContactBook_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly LogService _logService;

        public LogsController(LogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await _logService.GetAllAsync();

            return Ok(logs);
        }
    }
}
