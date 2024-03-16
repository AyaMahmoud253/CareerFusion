// JobSearchService.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Web_API.Models;
using Web_API.Services;

namespace Web_API.Services
{
    public class JobSearchService : IJobSearchService
    {
        private readonly ApplicationDBContext _context;

        public JobSearchService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobFormEntity>> SearchOpenPositions(string keyword)
        {
            // Query the database using EF Core to search for open positions
            var openPositions = await _context.JobForms.ToListAsync();

            // Convert keyword to lowercase for case-insensitive comparison
            string keywordLower = keyword?.ToLower();

            // Apply search filter based on keyword
            if (!string.IsNullOrWhiteSpace(keywordLower))
            {
                openPositions = openPositions.Where(p => p.JobTitle.ToLower().Contains(keywordLower)).ToList();
            }

            // Return the filtered list of open positions
            return openPositions;
        }

        public async Task<IEnumerable<JobFormEntity>> SearchOpenPositionsWithLocation(string keyword, string location)
        {
            // Query the database using EF Core to search for open positions
            var openPositions = await _context.JobForms.ToListAsync();

            // Convert keyword and location to lowercase for case-insensitive comparison
            string keywordLower = keyword?.ToLower();
            string locationLower = location?.ToLower();

            // Apply search filters based on keyword and location
            if (!string.IsNullOrWhiteSpace(keywordLower))
            {
                openPositions = openPositions.Where(p => p.JobTitle.ToLower().Contains(keywordLower)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(locationLower))
            {
                openPositions = openPositions.Where(p => p.JobLocation.ToLower().Contains(locationLower)).ToList();
            }

            // Return the filtered list of open positions
            return openPositions;
        }
    }
}
