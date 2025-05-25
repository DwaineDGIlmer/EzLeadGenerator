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
#pragma warning disable IDE0060 // Remove unused parameter
        [FromQuery] string engine)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            await Task.Delay(10);

            var result = new
            {
                organic_results = new object[]
                {
            new {
                position = 1,
                title = "Jane Doe - Data Engineer at TechCorp",
                link = "https://www.linkedin.com/in/janedoe",
                displayed_link = "linkedin.com/in/janedoe",
                snippet = "Jane Doe is a Data Engineer at TechCorp. Contact: jane.doe@gmail.com, (555) 123-4567"
            },
            new {
                position = 2,
                title = "John Smith | Data Engineer | GitHub",
                link = "https://github.com/johnsmith",
                displayed_link = "github.com/johnsmith",
                snippet = "Open source Data Engineer. Email: john.smith@yahoo.com, Phone: 555-987-6543"
            },
            new {
                position = 3,
                title = "Emily Zhang - Data Engineer - YouTube",
                link = "https://www.youtube.com/emilyzhang",
                displayed_link = "youtube.com/emilyzhang",
                snippet = "Emily Zhang shares Data Engineering tutorials. Reach at emily.zhang@gmail.com or 555.222.3333"
            },
            new {
                position = 4,
                title = "Carlos Rivera | Data Engineer | Instagram",
                link = "https://instagram.com/carlos.rivera",
                displayed_link = "instagram.com/carlos.rivera",
                snippet = "Carlos Rivera, Data Engineer. Contact: carlos.rivera@yahoo.com, (555) 444-5555"
            },
            new {
                position = 5,
                title = "Priya Patel - Data Engineer - TikTok",
                link = "https://tiktok.com/@priyapatel",
                displayed_link = "tiktok.com/@priyapatel",
                snippet = "Priya Patel creates Data Engineering content. Email: priya.patel@gmail.com"
            },
            new {
                position = 6,
                title = "<b>Jane Doe</b> - Freelance Designer",
                // link is missing to test fallback
                displayed_link = "jane.design",
                // snippet is missing to test fallback and skipping
            },
            new {
                position = 7,
                // No title, but has 'name' to test alternate property
                name = "Alternate Name Field",
                link = "https://alternate.example.com",
                displayed_link = "alternate.example.com",
                snippet = "No email here, just a phone: (123) 456-7890"
            },
            new {
                position = 8,
                // No title, but has 'name' to test alternate property
                name = "Martin Short",
                link = "https://facebook.com",
                displayed_link = "facebook.com",
                snippet = "No email here, just a phone: (123) 456-7890"
            },
            new {
                position = 9,
                // No title, but has 'name' to test alternate property
                name = "Eric Cartman",
                link = "https://twitter.com",
                displayed_link = "twitter.com",
                snippet = " phone: (123) 456-7890"
                }
            }
            };

            return Ok(result);
        }

        [HttpGet("Jobs")]
        public async Task<IActionResult> GetJobs(
#pragma warning disable IDE0060 // Remove unused parameter
        [FromQuery] string engine)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            await Task.Delay(10);

            var result = new
            {
                jobs_results = new object[]
                {
            new {
                title = "AI Engineer",
                company_name = "OpenAI",
                location = "Remote",
                extensions = new[] { "Full-time", "Remote" },
                detected_extensions = new { posted_at = "2 days ago" },
                description = "Build and optimize AI systems for human alignment.",
                apply_options = new[] { new { link = "https://openai.com/careers/apply/ai-engineer" } }
            },
            new {
                title = "Machine Learning Engineer",
                company_name = "Anthropic",
                location = "San Francisco, CA",
                extensions = new[] { "Full-time" },
                detected_extensions = new { posted_at = "1 day ago" },
                description = "Join our research team to scale safe ML models.",
                apply_options = new[] { new { link = "https://anthropic.com/careers/ml-engineer" } }
            },
            new {
                title = "Data Scientist",
                company_name = "Google",
                location = "Mountain View, CA",
                extensions = new[] { "Internship" },
                detected_extensions = new { posted_at = "3 days ago" },
                description = "Analyze big data to improve Search and Ads.",
                apply_options = new[] { new { link = "https://careers.google.com/jobs/results/data-scientist" } }
            },
            new {
                title = "AI Researcher",
                company_name = "Meta",
                location = "Menlo Park, CA",
                extensions = new[] { "Contract" },
                detected_extensions = new { posted_at = "5 days ago" },
                description = "Advance state-of-the-art in neural networks.",
                apply_options = new[] { new { link = "https://www.metacareers.com/jobs/ai-researcher" } }
            },
            new {
                title = "Deep Learning Specialist",
                company_name = "NVIDIA",
                location = "Austin, TX",
                extensions = new[] { "Full-time" },
                detected_extensions = new { posted_at = "Today" },
                description = "Develop DL systems for real-time inference.",
                apply_options = new[] { new { link = "https://www.nvidia.com/en-us/about-nvidia/careers" } }
            },

            // Flawed Entries Below

            new {
                title = "",
                company_name = "Unknown Corp",
                location = "",
                extensions = Array.Empty<string>(),
                description = "Exciting opportunity.",
                apply_options = Array.Empty<object>()
            },
            new {
                title = "AI Consultant",
                company_name = "",
                location = "Remote",
                extensions = new[] { "Full-time" },
                detected_extensions = new { },
                apply_options = Array.Empty < object >()
            },
            new {
                title = "ML Research Intern",
                company_name = "Startup Labs",
                location = "",
                description = "Internship for final-year students.",
                apply_options = Array.Empty < object >()
            },
            new {
                title = "Data Analyst",
                company_name = "Acme Corp",
                location = "New York, NY",
                extensions = new[] { "Part-time" },
                apply_options = new[] { new { link = "" } },
                description = "Collect and analyze customer data."
            },
            new {
                title = "Researcher",
                company_name = "Black Box AI",
                location = "Remote"
                // missing extensions, posted_at, description, and apply link
            }
                }
            };

            return Ok(result);
        }
    }
}
