// IJobSearchService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Web_API.Models;

namespace Web_API.Services
{
    public interface IJobSearchService
    {
        Task<IEnumerable<JobFormEntity>> SearchOpenPositions(string keyword);
        Task<IEnumerable<JobFormEntity>> SearchOpenPositionsWithLocation(string keyword, string location);
    }
}
