﻿using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Web_API.Services
{
    public interface IPictureUploadService
    {
        Task<int> UploadPictureAsync(int postId, IFormFile picture);
        Task<string> GetPicturePathAsync(int pictureId);

    }
}
