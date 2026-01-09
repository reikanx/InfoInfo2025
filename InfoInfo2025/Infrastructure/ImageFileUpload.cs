using System.Drawing;
using LazZiya.ImageResize;

namespace InfoInfo2025.Infrastructure
{
    public class ImageFileUpload
    {
        private readonly IWebHostEnvironment hostingEnvironment;
        public ImageFileUpload(IWebHostEnvironment environment)
        {
            hostingEnvironment = environment;
        }

        public FileSendResult SendFile(IFormFile picture, string destination, int width)
        {
            var result = new FileSendResult();

            try
            {
                string extension = Path.GetExtension(picture.FileName);
                if (!FileTypeCheck(extension))
                {
                    result.Name = Path.GetFileName(picture.FileName);
                    result.Success = false;
                    result.Error = "Niepoprawny typ pliku graficznego.";
                    return result;
                }

                result.Name = Guid.NewGuid().ToString() + extension;
                var mainUploadPath = Path.Combine(hostingEnvironment.WebRootPath, destination);
                var miniUploadPath = Path.Combine(mainUploadPath, "mini");
                var mainFilePath = Path.Combine(mainUploadPath, result.Name);
                var miniFilePath = Path.Combine(miniUploadPath, result.Name);

                using (var fileStream = new FileStream(mainFilePath, FileMode.Create))
                {
                    picture.CopyTo(fileStream);
                }
                using (var image = Image.FromFile(mainFilePath))
                using (var miniature = image.ScaleByWidth(width))
                {
                    miniature.Save(miniFilePath);
                }

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = $"Wystąpił błąd podczas przesyłania pliku: {ex.Message}";
                return result;
            }

        }

        private static bool FileTypeCheck(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".png" or ".gif" or ".webp" => true,
                _ => false,
            };
        }
    }
}
