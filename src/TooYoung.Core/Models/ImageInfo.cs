using System.IO;

namespace TooYoung.Core.Models
{
    public class ImageInfo
    {
        public string Id { get; set; }
        
        public string GroupId { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>
        /// 图片大小，单位为字节
        /// </summary>
        /// <returns></returns>
        public int SizeOfBytes { get; set; }
        
        public string Image { get; set; }

        // TODO: EXIF, Thumbnail
    }

    public static class ImageInfoFunc
    {
        public static string GetMime(this ImageInfo info)
        {
            var ext = Path.GetExtension(info.Name).Trim('.');
            return $"image/{ext}";
        }
    }
}
