using Microsoft.AspNetCore.Http;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace IdeaShare.Application.Utilities
{
    public static class ImageUtilities
    {
        public static Bitmap ResizeImage(Bitmap bmp, int desiredWidth, int desiredHeight)
        {
            if(bmp.Width == desiredWidth && bmp.Height == desiredHeight)
            {
                return bmp;
            }

            if (desiredWidth <= 0)
            {
                throw new ArgumentOutOfRangeException("Desired width has to be greater than 0.", nameof(desiredHeight));
            }

            if (desiredHeight <= 0)
            {
                throw new ArgumentOutOfRangeException("Desired height has to be greater than 0.", nameof(desiredWidth));
            }

            var newImage = new Bitmap(desiredWidth, desiredHeight);

            using var graphics = Graphics.FromImage(newImage);
            graphics.DrawImage(bmp, 0, 0, desiredWidth, desiredHeight);

            return newImage;
        }

        public static async Task<string> ProcessUploadedFile(IFormFile formFile, string saveLocation, ImageFormat fileFormat, int desiredWidth, int desiredHeight)
        {
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string folder = Path.Combine(webRootPath, saveLocation);
            // Create the directory if it doesn't exist
            Directory.CreateDirectory(folder);
            string uniqueFileName = $"{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}.{fileFormat.ToString().ToLower()}";
            string filePath = Path.Combine(folder, uniqueFileName);
            using var fileStream = new FileStream(filePath, FileMode.Create);

            using var memStream = new MemoryStream();
            await formFile.CopyToAsync(memStream);

            var bmp = new Bitmap(memStream);

            // if both dimensions aren't ignored
            if(!(desiredWidth == -1 && desiredHeight == -1))
            {
                var newWidth = desiredWidth == -1 ? bmp.Width : desiredWidth;
                var newHeight = desiredHeight == -1 ? bmp.Height : desiredHeight;
                bmp = ResizeImage(bmp, newWidth, newHeight);
            }
            bmp.Save(fileStream, fileFormat);

            return uniqueFileName;
        }
    }
}
