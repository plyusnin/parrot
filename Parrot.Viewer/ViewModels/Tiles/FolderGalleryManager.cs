using System;
using System.IO;
using System.Linq;
using Parrot.Viewer.GallerySources;
using Parrot.Viewer.GallerySources.Exif;
using Parrot.Viewer.GallerySources.Thumbnails;

namespace Parrot.Viewer.ViewModels.Tiles
{
    public class FolderGalleryManager
    {
        private readonly ExifManager _exifManager = new ExifManager();
        private readonly IGallery _gallery;
        private readonly string _root;
        private readonly ThumbnailFactory _thumbnailFactory = new ThumbnailFactory();

        public FolderGalleryManager(IGallery Gallery, string Root)
        {
            _gallery = Gallery;
            _root = Root;

            Directory.EnumerateFiles(_root, "*.jpg", SearchOption.AllDirectories)
                     .Where(f => !_gallery.Contains(f))
                     .AsParallel()
                     .Select(Load)
                     .Where(x => x.entity != null && x.thumbnail != null)
                     .ForAll(x => Gallery.Add(x.entity, x.thumbnail));
        }

        private (IPhotoEntity entity, Stream thumbnail) Load(string FileName)
        {
            try
            {
                var exif = _exifManager.Load(FileName);
                var entity = new PhotoEntity(FileName, exif);
                var thumbnail = new MemoryStream(_thumbnailFactory.GenerateThumbnail(FileName, exif.Rotation));
                return (entity, thumbnail);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return (null, null);
            }
        }
    }
}
