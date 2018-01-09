using System.Globalization;
using System.IO;
using ExifLib;
using Geographics;

namespace Parrot.Viewer.GallerySources.Exif
{
    public class ExifManager
    {
        public ExifInformation Load(string FileName)
        {
            using (var reader = new ExifReader(FileName))
            {
                var aperture     = reader.GetTagValueOrDefault<double, string>(ExifTags.FNumber,      a => $"f/{a.ToString(CultureInfo.InvariantCulture)}");
                var shutterSpeed = reader.GetTagValueOrDefault<double, string>(ExifTags.ExposureTime, v => v < 1 ? $"1/{1 / v}" : $"{v}\"");
                var iso          = reader.GetTagValueOrDefault(ExifTags.PhotographicSensitivity)?.ToString();
                var camera       = $"{reader.GetTagValueOrDefault<string>(ExifTags.Make)} {reader.GetTagValueOrDefault<string>(ExifTags.Model)}";
                var shotTime     = reader.GetTagValueOrDefault(ExifTags.DateTimeDigitized, File.GetCreationTime(FileName));

                var gps = GetGpsPosition(reader);

                return new ExifInformation(aperture, shutterSpeed, iso, camera, shotTime, gps);
            }
        }

        private static EarthPoint? GetGpsPosition(ExifReader reader)
        {
            var latitudeRef  = reader.GetTagValueOrDefault(ExifTags.GPSLatitudeRef,  "N");
            var longitudeRef = reader.GetTagValueOrDefault(ExifTags.GPSLongitudeRef, "E");
            var latitude     = reader.GetTagValueOrDefault<double[]>(ExifTags.GPSLatitude);
            var longitude    = reader.GetTagValueOrDefault<double[]>(ExifTags.GPSLongitude);

            if (latitude == null || longitude == null)
                return null;

            var lam = latitudeRef.ToLower() == "s" ? -1 : 1;
            var lom = longitudeRef.ToLower() == "w" ? -1 : 1;

            return new EarthPoint(new Degree(lam * (int)latitude[0],  latitude[1],  latitude[2]),
                                  new Degree(lom * (int)longitude[0], longitude[1], longitude[2]));
        }
    }
}
