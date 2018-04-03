using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using Geographics;
using LiteDB;
using Parrot.Viewer.GallerySources.Database.Entities;
using Parrot.Viewer.GallerySources.Exif;
using Parrot.Viewer.GallerySources.Thumbnails;
using ReactiveUI;

namespace Parrot.Viewer.GallerySources.Database
{
    public class DatabaseGallerySource : IGallerySource, IDisposable
    {
        private readonly LiteDatabase _db;
        private readonly CompositeDisposable _disposeOnExit = new CompositeDisposable();
        private readonly ExifManager _exifManager = new ExifManager();
        private readonly LiteCollection<DbPhotoRecord> _photos;

        private readonly string _root;
        private readonly ThumbnailFactory _thumbnailFactory = new ThumbnailFactory();

        public DatabaseGallerySource(string Root)
        {
            _root = Root;
            _db = new LiteDatabase("gallery.db")
                .DisposeWith(_disposeOnExit);

            _photos = _db.GetCollection<DbPhotoRecord>("photos");

            Photos = new ReactiveList<IPhotoEntity>();

            Directory.EnumerateFiles(_root, "*.jpg", SearchOption.AllDirectories)
                     .AsParallel()
                     .Select(Load)
                     .Where(r => r != null)
                     .ToList()
                     .ForEach(Photos.Add);
        }

        public void Dispose() { _disposeOnExit.Dispose(); }

        public IReactiveList<IPhotoEntity> Photos { get; }

        private DatabasePhotoEntity Load(string FileName)
        {
            try
            {
                Debug.Print($"--> {FileName}");

                var record = _photos.FindOne(p => p.FileName == FileName);
                if (record == null)
                {
                    var exif = _exifManager.Load(FileName);
                    record = new DbPhotoRecord
                             {
                                 FileName = FileName,
                                 Aperture = exif.Aperture,
                                 Camera = exif.Camera,
                                 Iso = exif.Iso,
                                 ShotTime = exif.ShotTime,
                                 ShutterSpeed = exif.ShutterSpeed,
                                 hasGps = exif.Gps != null,
                                 Latitude = exif.Gps?.Latitude.ToE6Int() ?? 0,
                                 Longitude = exif.Gps?.Longitude.ToE6Int() ?? 0,
                                 Rotation = exif.Rotation
                             };
                    _photos.Insert(record);
                    _photos.EnsureIndex(x => x.FileName);
                }

                if (!_db.FileStorage.Exists($"$/thumbnails/{record.Id}.jpg"))
                {
                    var thumbnail = new MemoryStream(_thumbnailFactory.GenerateThumbnail(FileName, record.Rotation));
                    _db.FileStorage.Upload($"$/thumbnails/{record.Id}.jpg", $"{record.Id}.jpg",
                                           thumbnail);
                }

                var gps = record.hasGps
                              ? new EarthPoint(Degree.FromE6Int(record.Latitude),
                                               Degree.FromE6Int(record.Longitude))
                              : (EarthPoint?)null;

                return new DatabasePhotoEntity(_db,
                                               record.Id,
                                               FileName,
                                               new ExifInformation(record.Aperture,
                                                                   record.ShutterSpeed,
                                                                   record.Iso,
                                                                   record.Camera,
                                                                   record.ShotTime,
                                                                   gps,
                                                                   record.Rotation));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}
