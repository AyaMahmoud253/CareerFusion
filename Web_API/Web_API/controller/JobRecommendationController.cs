using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobRecommendationController : ControllerBase
    {
        private readonly HttpClient _client;

        public JobRecommendationController()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:5000"); // Assuming Flask is running on port 5000
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetRecommendedJobs(string userId)
        {
            try
            {
                // Call the Flask endpoint
                var response = await _client.GetAsync($"/recommend-jobs/{userId}");
                response.EnsureSuccessStatusCode();

                // Read the response content
                var responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON response
                var recommendedJobs = JsonConvert.DeserializeObject<List<RecommendedJob>>(responseBody);

                return Ok(recommendedJobs);
            }
            catch (HttpRequestException)
            {
                return StatusCode(500, "Failed to fetch recommended jobs from Flask server.");
            }
            catch (JsonException)
            {
                return StatusCode(500, "Failed to parse JSON response from Flask server.");
            }
        }
    }

    public class RecommendedJob
    {
        public int Id { get; set; }
        public string JobTitle { get; set; }
        public string JobType { get; set; }
        public string JobLocation { get; set; }
        public decimal Similarity { get; set; }
        // Add other properties as needed
    }
}
