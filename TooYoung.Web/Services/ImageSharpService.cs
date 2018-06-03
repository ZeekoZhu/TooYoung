using System.IO;
using CSharpFunctionalExtensions;
using SixLabors.ImageSharp;
using TooYoung.Core.Services;

namespace TooYoung.Web.Services
{
    public class ImageSharpService : IImageProcessService
    {
        public Result<(int Width, int Height)> GetBound(MemoryStream bin)
        {
            var result = Maybe<IImageInfo>
                .From(Image.Identify(bin))
                .ToResult("Can not identify image format")
                .Map(info => (info.Width, info.Height));
            return result;
        }
    }
}
