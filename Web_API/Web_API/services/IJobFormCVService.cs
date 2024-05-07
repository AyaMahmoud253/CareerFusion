using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web_API.Services
{
    public interface IJobFormCVService
    {
        Task<IEnumerable<string>> GetCVFilePathsByTitleAsync(string jobTitle);
    }
}
