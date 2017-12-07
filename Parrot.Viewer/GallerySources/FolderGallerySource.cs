using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using photo.exif;
using ReactiveUI;

namespace Parrot.Viewer.GallerySources
{
    public class FolderGallerySource : IGallerySource
    {
        private readonly Parser _parser = new Parser();
        private readonly string _root;
        private readonly double _thumbnailSize = 300.0;

        public FolderGallerySource(string Root)
        {
            _root = Root;
            Photos = new ReactiveList<IPhotoEntity>(
                Directory.EnumerateFiles(_root, "*.jpg")
                         .Select(OpenPhoto));
        }

        public IReactiveList<IPhotoEntity> Photos { get; }

        private IPhotoEntity OpenPhoto(string File)
        {
            var items = _parser.Parse(File)
                               .ToDictionary(x => x.Id);
            var image = Image.FromFile(File);

            var factor = Math.Max(_thumbnailSize / image.Width, _thumbnailSize / image.Height);

            var thumb = new Bitmap(image, new Size((int)(image.Width * factor), (int)(image.Height * factor)));
            var ms = new MemoryStream();
            thumb.Save(ms, ImageFormat.Jpeg);
            return new MemoryPhotoEntity(items, ms.ToArray());
        }
    }
}
