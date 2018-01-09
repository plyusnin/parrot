using System;
using Geographics;

namespace Parrot.Viewer.GallerySources.Exif
{
    public class ExifInformation
    {
        public ExifInformation(string Aperture, string ShutterSpeed, string Iso, string Camera, DateTime ShotTime, EarthPoint? Gps)
        {
            this.Aperture     = Aperture;
            this.ShutterSpeed = ShutterSpeed;
            this.Iso          = Iso;
            this.Camera       = Camera;
            this.ShotTime     = ShotTime;
            this.Gps          = Gps;
        }

        public string      Aperture     { get; }
        public string      ShutterSpeed { get; }
        public string      Iso          { get; }
        public string      Camera       { get; }
        public DateTime    ShotTime     { get; }
        public EarthPoint? Gps          { get; }
    }
}
