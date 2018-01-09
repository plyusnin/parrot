using System.IO;
using LiteDB;
using Parrot.Viewer.GallerySources.Exif;

namespace Parrot.Viewer.GallerySources.Database
{
    public class DatabasePhotoEntity : IPhotoEntity
    {
        private readonly LiteDatabase _database;
        private readonly int _photoId;

        public DatabasePhotoEntity(LiteDatabase Database, int PhotoId, string FileName, ExifInformation Exif)
        {
            _database = Database;
            this.FileName = FileName;
            this.Exif = Exif;
            _photoId = PhotoId;
        }

        public ExifInformation Exif { get; }
        public string FileName { get; }

        public Stream OpenThumbnail() { return _database.FileStorage.OpenRead($"$/thumbnails/{_photoId}.jpg"); }
    }
}
