using System.IO;
using CSharpFunctionalExtensions;

namespace TooYoung.Core.Services
{
    public interface IImageProcessService
    {
        Result<(int Width, int Height)> GetBound(MemoryStream bin);
    }
}
