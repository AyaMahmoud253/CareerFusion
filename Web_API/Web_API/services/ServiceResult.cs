using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Web_API.Models;

namespace Web_API.services
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}