using System;

namespace Parrot.Viewer.GallerySources.Database.Entities
{
    public class DbPhotoRecord
    {
        public int    Id       { get; set; }
        public string FileName { get; set; }

        public string   Aperture     { get; set; }
        public string   ShutterSpeed { get; set; }
        public string   Iso          { get; set; }
        public string   Camera       { get; set; }
        public DateTime ShotTime     { get; set; }

        public int  Latitude  { get; set; }
        public int  Longitude { get; set; }
        public bool hasGps    { get; set; }
    }
}
