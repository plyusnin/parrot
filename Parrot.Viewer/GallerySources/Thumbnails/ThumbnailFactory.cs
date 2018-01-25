using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Parrot.Viewer.GallerySources.Thumbnails
{
    public class ThumbnailFactory
    {
        private readonly double _thumbnailSize = 240.0;

        public byte[] GenerateThumbnail(string FileName, int Rotation)
        {
            using (var ms = new MemoryStream())
            {
                using (var image = Image.FromFile(FileName))
                {
                    var factor = Math.Max(_thumbnailSize / image.Width, _thumbnailSize / image.Height);

                    using (var thumb = new Bitmap(image, new Size((int)(image.Width * factor), (int)(image.Height * factor))))
                    {
                        switch (Rotation)
                        {
                            case 0: break;
                            case 90:
                                thumb.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;
                            case 180:
                                thumb.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                break;
                            case 270:
                                thumb.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                            default: throw new ArgumentException("Не умеем поворачивать на угол, не кратный 90 градусам", nameof(Rotation));
                        }

                        thumb.Save(ms, ImageFormat.Jpeg);
                    }

                    return ms.ToArray();
                }
            }
        }
    }
}
