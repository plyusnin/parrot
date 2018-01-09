using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Parrot.Viewer.GallerySources.Thumbnails
{
    public class ThumbnailFactory
    {
        private readonly double _thumbnailSize = 240.0;

        public byte[] GenerateThumbnail(string FileName)
        {
            using (var ms = new MemoryStream())
            {
                using (var image = Image.FromFile(FileName))
                {
                    var factor = Math.Max(_thumbnailSize / image.Width, _thumbnailSize / image.Height);

                    using (var thumb = new Bitmap(image, new Size((int)(image.Width * factor), (int)(image.Height * factor))))
                    {
                        thumb.Save(ms, ImageFormat.Jpeg);
                    }

                    return ms.ToArray();
                }
            }
        }
    }
}
