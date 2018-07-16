using System.IO;
using TooYoung.Core.Exceptions;

namespace TooYoung.Core.Services
{
    /// <summary>
    /// 读取图片的宽度与高度
    /// </summary>
    /// <exception cref="BlogAppException"></exception>
    public interface IImageProcessService
    {
        (int Width, int Height) GetBound(MemoryStream bin);
    }
}
