using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Web_API.Models;
using Web_API.Services;
namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenPosCVController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public OpenPosCVController(ApplicationDBContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

    }      
}
