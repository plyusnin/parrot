using System;

namespace Parrot.Viewer.GallerySources.Database.Entities
{
    public class DbGeotag
    {
        public int Id { get; set; }
        public int PhotoId { get; set; }
        public DateTime ShotTime { get; set; }

        public int Scale { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public override string ToString()
        {
            return $"{nameof(Scale)} {Scale}: {X}-{Y}";
        }
    }
}
