using Microsoft.AspNetCore.Mvc;

namespace Harness.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EzLeadsController : Controller
    {
        [HttpGet(Name = "GetStatus")]
        public IActionResult Get()
        {
            return Ok("Service is running.");
        }

        [HttpGet("leads")]
        public async Task<IActionResult> GetLeads(
        [FromQuery] string engine)
        {
            await Task.Delay(10);

            if (!string.IsNullOrWhiteSpace(engine))
            {
                if (engine == "google_jobs")
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "talentresults.json");
                    if (!System.IO.File.Exists(filePath))
                        return NotFound("Lead results file not found.");

                    var json = await System.IO.File.ReadAllTextAsync(filePath);
                    return Content(json, "application/json");
                }
                if (engine == "google")
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "leadresults.json");
                    if (!System.IO.File.Exists(filePath))
                        return NotFound("Lead results file not found.");

                    var json = await System.IO.File.ReadAllTextAsync(filePath);
                    return Content(json, "application/json");
                }
            }

            return BadRequest("Invalid or missing 'engine' query parameter.");
        }
    }
}
