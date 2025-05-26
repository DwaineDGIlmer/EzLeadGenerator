using Microsoft.AspNetCore.Mvc;

namespace Harness.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EzLeadsController : Controller
    {
        private const string _leadResults = "{\r\n  \"search_metadata\": {\r\n    \"id\": \"66c3556d2c83d58253c17c91\",\r\n    \"status\": \"Success\",\r\n    \"json_endpoint\": \"https://serpapi.com/searches/66c3556d2c83d58253c17c91.json\",\r\n    \"created_at\": \"2024-10-30 07:02:00 UTC\",\r\n    \"processed_at\": \"2024-10-30 07:02:00 UTC\",\r\n    \"google_url\": \"https://www.google.com/search?q=Coffee&hl=en&gl=us&sourceid=chrome&ie=UTF-8\",\r\n    \"total_time_taken\": 1.52\r\n  },\r\n  \"search_parameters\": {\r\n    \"engine\": \"google\",\r\n    \"q\": \"Coffee\",\r\n    \"location_requested\": \"Austin, Texas, United States\",\r\n    \"location_used\": \"Austin,Texas,United States\",\r\n    \"google_domain\": \"google.com\",\r\n    \"hl\": \"en\",\r\n    \"gl\": \"us\",\r\n    \"device\": \"desktop\"\r\n  },\r\n  \"search_information\": {\r\n    \"organic_results_state\": \"Results for exact spelling\",\r\n    \"query_displayed\": \"Coffee\",\r\n    \"total_results\": 1340000000,\r\n    \"time_taken_displayed\": 0.99\r\n  },\r\n  \"organic_results\": [\r\n    {\r\n      \"position\": 1,\r\n      \"title\": \"Starbucks Coffee Company\",\r\n      \"link\": \"https://www.starbucks.com/\",\r\n      \"displayed_link\": \"https://www.starbucks.com\",\r\n      \"snippet\": \"More than just great coffee. Explore the menu, sign up for Starbucks® Rewards, manage your gift card and more.\"\r\n    },\r\n    {\r\n      \"position\": 2,\r\n      \"title\": \"Coffee - Wikipedia\",\r\n      \"link\": \"https://en.wikipedia.org/wiki/Coffee\",\r\n      \"displayed_link\": \"https://en.wikipedia.org › wiki › Coffee\",\r\n      \"snippet\": \"Coffee is a brewed drink prepared from roasted coffee beans, the seeds of berries from certain flowering plants in the Coffea genus.\"\r\n    },\r\n    {\r\n      \"position\": 3,\r\n      \"title\": \"Peet's Coffee: The Original Craft Coffee\",\r\n      \"link\": \"https://www.peets.com/\",\r\n      \"displayed_link\": \"https://www.peets.com\",\r\n      \"snippet\": \"Since 1966, Peet's Coffee has offered superior coffees and teas by sourcing the best quality coffee beans and tea leaves in the world and adhering to strict high-quality and taste standards.\"\r\n    }\r\n]\r\n}";

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
