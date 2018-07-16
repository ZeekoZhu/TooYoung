using System.IO;
using SixLabors.ImageSharp;
using TooYoung.Core.Exceptions;
using TooYoung.Core.Services;

namespace TooYoung.Web.Services
{
    public class ImageSharpService : IImageProcessService
    {
        public (int Width, int Height) GetBound(MemoryStream bin)
        {
            var result = Image.Identify(bin);
            if (result == null)
            {
                throw new AppException("Can not identify image format");
            }

            return (result.Width, result.Height);
        }
    }
}
