using System;

namespace Parrot.Viewer.GallerySources
{
    public class ExifInformation
    {
        public ExifInformation(string Aperture, string ShutterSpeed, string Iso, string Camera, DateTime ShotTime)
        {
            this.Aperture = Aperture;
            this.ShutterSpeed = ShutterSpeed;
            this.Iso = Iso;
            this.Camera = Camera;
            this.ShotTime = ShotTime;
        }

        public string Aperture { get; }
        public string ShutterSpeed { get; }
        public string Iso { get; }
        public string Camera { get; }
        public DateTime ShotTime { get; }
    }
}
