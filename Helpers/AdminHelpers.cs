using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FashionShop.Helpers
{
    public static class AdminHelpers
    {
        /// <summary>
        /// Checks if the current user has admin permission
        /// </summary>
        /// <param name="session">The HttpContext.Session</param>
        /// <returns>True if the user is an admin, otherwise false</returns>
        public static bool IsAdmin(ISession session)
        {
            return session.GetInt32("permission") == 1;
        }

        /// <summary>
        /// Saves an uploaded file to disk and returns the path
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <param name="folder">The subfolder within images directory</param>
        /// <returns>The relative path to the saved image</returns>
        public static async Task<string> SaveImage(IFormFile file, string folder = "")
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine(folder, uniqueFileName).Replace("\\", "/");
        }
    }
}