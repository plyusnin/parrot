using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using Geographics;
using LiteDB;
using Parrot.Viewer.GallerySources.Database.Entities;
using Parrot.Viewer.GallerySources.Exif;

namespace Parrot.Viewer.GallerySources.Database
{
    public class DatabaseGallery : IGallery, IDisposable
    {
        private readonly LiteDatabase _db;
        private readonly CompositeDisposable _disposeOnExit = new CompositeDisposable();
        private readonly IGeoIndexer _geoIndexer;
        private readonly LiteCollection<DbGeotag> _geotags;
        private readonly LiteCollection<DbPhotoRecord> _photos;

        public DatabaseGallery(IGeoIndexer GeoIndexer)
        {
            _geoIndexer = GeoIndexer;
            _db = new LiteDatabase("gallery.db")
                .DisposeWith(_disposeOnExit);

            //BsonMapper.Global.Entity<DbGeotag>()
            //          .DbRef(x => x.Photo, "photos");

            _photos = _db.GetCollection<DbPhotoRecord>("photos");
            _geotags = _db.GetCollection<DbGeotag>("geotags");
        }

        public void Dispose()
        {
            _disposeOnExit.Dispose();
        }

        public IList<IPhotoEntity> WithinInterval(DateTime From, DateTime To)
        {
            throw new NotImplementedException();
        }

        public bool Contains(string FileName)
        {
            return _photos.Exists(f => f.FileName == FileName);
        }

        public void Add(IPhotoEntity Entity, Stream Thumbnail)
        {
            var record = new DbPhotoRecord
            {
                FileName = Entity.FileName,
                Aperture = Entity.Exif.Aperture,
                Camera = Entity.Exif.Camera,
                Iso = Entity.Exif.Iso,
                ShotTime = Entity.Exif.ShotTime,
                ShutterSpeed = Entity.Exif.ShutterSpeed,
                hasGps = Entity.Exif.Gps != null,
                Latitude = Entity.Exif.Gps?.Latitude.ToE6Int() ?? 0,
                Longitude = Entity.Exif.Gps?.Longitude.ToE6Int() ?? 0,
                Rotation = Entity.Exif.Rotation
            };
            _photos.Insert(record);
            _photos.EnsureIndex(x => x.FileName);
            _photos.EnsureIndex(x => x.ShotTime);

            if (Entity.Exif.Gps != null)
            {
                var geotags = _geoIndexer.ListGeotagsFor(Entity.Exif.Gps.Value)
                                         .Select(t => new DbGeotag
                                         {
                                             Scale = t.Scale,
                                             X = t.X,
                                             Y = t.Y,
                                             ShotTime = Entity.Exif.ShotTime,
                                             Longitude = Entity.Exif.Gps.Value.Longitude,
                                             Latitude = Entity.Exif.Gps.Value.Latitude,
                                             PhotoId = record.Id
                                         });
                _geotags.Insert(geotags);
            }
            _geotags.EnsureIndex(x => x.PhotoId);
            _geotags.EnsureIndex(x => x.Scale);
            _geotags.EnsureIndex(x => x.X);
            _geotags.EnsureIndex(x => x.Y);
            _geotags.EnsureIndex(x => x.ShotTime);

            _db.FileStorage.Upload($"$/thumbnails/{record.Id}.jpg", $"{record.Id}.jpg", Thumbnail);
            Console.WriteLine($" --> Added to Galery: {Entity.FileName}");
        }

        public IList<IPhotoEntity> All()
        {
            return All(0, int.MaxValue);
        }

        public Stream OpenThumbnail(IPhotoEntity PhotoEntity)
        {
            var recordId = PhotoEntity is DatabasePhotoEntity dbEntity
                               ? dbEntity.Id
                               : _photos.FindOne(e => e.FileName == PhotoEntity.FileName).Id;
            return _db.FileStorage.OpenRead($"$/thumbnails/{recordId}.jpg");
        }

        public IList<IPhotoEntity> All(int Offset, int Count)
        {
            return _photos.Find(Query.All(nameof(DbPhotoRecord.ShotTime), -1), Offset, Count)
                          .Select(ToPhotoEntity)
                          .ToList();
        }

        public IList<GeoStack> GetPhotosOnMap(int Scale, int FromX, int ToX, int FromY, int ToY)
        {
            var tags = _geotags.Find(t => t.Scale == Scale && t.X >= FromX && t.X <= ToX && t.Y >= FromY && t.Y <= ToY)
                               .OrderByDescending(t => t.ShotTime)
                               .GroupBy(t => new { t.X, t.Y })
                               .ToList();
            return tags.Where(tg => tg.Any())
                       .Select(tg => new GeoStack(tg.Select(t => new EarthPoint(t.Latitude, t.Longitude)).ToList(),
                                                  ToPhotoEntity(_photos.FindById(tg.First().PhotoId))))
                       .ToList();
        }

        public IList<GeoStack> GetPhotosOnMap(int Scale, EarthArea ForArea)
        {
            return GetPhotosOnMap(Scale,
                                  _geoIndexer.GetX(ForArea.MostWesternLongitude, Scale),
                                  _geoIndexer.GetX(ForArea.MostEasternLongitude, Scale),
                                  _geoIndexer.GetY(ForArea.MostNorthenLatitude, Scale),
                                  _geoIndexer.GetY(ForArea.MostSouthernLatitude, Scale));
        }

        private IPhotoEntity ToPhotoEntity(DbPhotoRecord Record)
        {
            var gps = Record.hasGps
                          ? new EarthPoint(Degree.FromE6Int(Record.Latitude),
                                           Degree.FromE6Int(Record.Longitude))
                          : (EarthPoint?)null;

            return new DatabasePhotoEntity(Record.Id, Record.FileName,
                                           new ExifInformation(Record.Aperture,
                                                               Record.ShutterSpeed,
                                                               Record.Iso,
                                                               Record.Camera,
                                                               Record.ShotTime,
                                                               gps,
                                                               Record.Rotation));
        }

        private class DatabasePhotoEntity : PhotoEntity
        {
            public DatabasePhotoEntity(int Id, string FileName, ExifInformation Exif) : base(FileName, Exif)
            {
                this.Id = Id;
            }

            public int Id { get; }
        }
    }
}
