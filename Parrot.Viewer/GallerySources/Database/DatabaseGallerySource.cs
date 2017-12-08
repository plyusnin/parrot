using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using photo.exif;
using Parrot.Viewer.GallerySources.Database.Entities;
using ReactiveUI;

namespace Parrot.Viewer.GallerySources.Database
{
    public class DatabaseGallerySource : IGallerySource, IDisposable
    {
        private ExifManager _exifManager = new ExifManager();
        private readonly IFileGallerySource _core;
        private readonly GalleryContext _db;
        private readonly CompositeDisposable _disposeOnExit = new CompositeDisposable();
        private readonly double _thumbnailSize = 300.0;

        public DatabaseGallerySource(IFileGallerySource Core)
        {
            Photos = new ReactiveList<IPhotoEntity>();
            _db = new GalleryContext()
                .DisposeWith(_disposeOnExit);

            _core = Core;
            foreach (var photo in _core.Photos)
                OnCorePhotoAdded(photo);

            _core.Photos.ItemsAdded
                 .SubscribeOn(TaskPoolScheduler.Default)
                 .Subscribe(OnCorePhotoAdded)
                 .DisposeWith(_disposeOnExit);
        }

        public void Dispose() { _disposeOnExit.Dispose(); }

        public IReactiveList<IPhotoEntity> Photos { get; }

        private void OnCorePhotoAdded(FilePhotoRecord Photo)
        {
            var fileName = Photo.FileName;
            var record = _db.Photos.FirstOrDefault(f => f.File == fileName);
            if (record == null)
            {
                var image = Image.FromFile(fileName);
                var factor = Math.Max(_thumbnailSize / image.Width, _thumbnailSize / image.Height);

                var thumb = new Bitmap(image, new Size((int)(image.Width * factor), (int)(image.Height * factor)));
                var ms = new MemoryStream();
                thumb.Save(ms, ImageFormat.Jpeg);


                record = new DbPhotoRecord
                         {
                             File = fileName,
                             Thumbnail = ms.ToArray()
                         };

                _db.Photos.Add(record);
                _db.SaveChanges();
            }

            var exif = _exifManager.Load(fileName);
            var entity = new PhotoEntity(fileName, record.Thumbnail, exif);
            Photos.Add(entity);
        }
    }
}
