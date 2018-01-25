﻿using System;
using System.Diagnostics;
using System.IO;
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
        private readonly IFileGallerySource _core;
        private readonly LiteDatabase _db;
        private readonly CompositeDisposable _disposeOnExit = new CompositeDisposable();
        private readonly ExifManager _exifManager = new ExifManager();
        private readonly LiteCollection<DbPhotoRecord> _photos;
        private readonly ThumbnailFactory _thumbnailFactory = new ThumbnailFactory();

        public DatabaseGallerySource(IFileGallerySource Core)
        {
            _db = new LiteDatabase("gallery.db")
                .DisposeWith(_disposeOnExit);

            _photos = _db.GetCollection<DbPhotoRecord>("photos");

            _core  = Core;
            Photos = _core.Photos
                          .CreateDerivedCollection(Load)
                          .DisposeWith(_disposeOnExit);
        }

        public void Dispose()
        {
            _disposeOnExit.Dispose();
        }

        public IReactiveDerivedList<IPhotoEntity> Photos { get; }

        private DatabasePhotoEntity Load(FilePhotoRecord Photo)
        {
            try
            {
                Debug.Print($"--> {Photo.FileName}");

                var fileName = Photo.FileName;
                var record   = _photos.FindOne(p => p.FileName == fileName);
                if (record == null)
                {
                    var exif = _exifManager.Load(fileName);
                    record   = new DbPhotoRecord
                    {
                        FileName     = fileName,
                        Aperture     = exif.Aperture,
                        Camera       = exif.Camera,
                        Iso          = exif.Iso,
                        ShotTime     = exif.ShotTime,
                        ShutterSpeed = exif.ShutterSpeed,
                        hasGps       = exif.Gps != null,
                        Latitude     = exif.Gps?.Latitude.ToE6Int() ?? 0,
                        Longitude    = exif.Gps?.Longitude.ToE6Int() ?? 0,
                        Rotation     = exif.Rotation
                    };
                    _photos.Insert(record);
                    _photos.EnsureIndex(x => x.FileName);
                }

                if (!_db.FileStorage.Exists($"$/thumbnails/{record.Id}.jpg"))
                {
                    var thumbnail = new MemoryStream(_thumbnailFactory.GenerateThumbnail(fileName, record.Rotation));
                    _db.FileStorage.Upload($"$/thumbnails/{record.Id}.jpg", $"{record.Id}.jpg",
                                           thumbnail);
                }

                var gps = record.hasGps
                              ? new EarthPoint(Degree.FromE6Int(record.Latitude),
                                               Degree.FromE6Int(record.Longitude))
                              : (EarthPoint?)null;

                return new DatabasePhotoEntity(_db,
                                               record.Id,
                                               fileName,
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
                throw;
            }
        }
    }
}
