using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web_API.Models;

namespace Web_API.Services
{
    public class JobFormCVService : IJobFormCVService
    {
        private readonly ApplicationDBContext _context;

        public JobFormCVService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<string>> GetCVFilePathsByTitleAsync(string jobTitle)
        {
            var cvFilePaths = await _context.JobFormCVs
                .Include(cv => cv.JobForm)
                .Where(cv => cv.JobForm.JobTitle == jobTitle)
                .Select(cv => cv.FilePath)
                .ToListAsync();

            return cvFilePaths;
        }
    }
}
