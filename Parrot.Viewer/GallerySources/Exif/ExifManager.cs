using System;
using System.Globalization;
using System.IO;
using System.Linq;
using photo.exif;

namespace Parrot.Viewer.GallerySources.Exif
{
    public class ExifManager
    {
        private readonly Parser _parser = new Parser();

        public ExifInformation Load(string FileName)
        {
            var exif = _parser.Parse(FileName).ToDictionary(x => x.Id);

            var a = (URational)exif[0x829d].Value;
            var aString = ((double)a.Numerator / a.Denominator).ToString(CultureInfo.InvariantCulture);

            var time = exif[36867].Value.ToString().TrimEnd('\0');

            if (!DateTime.TryParseExact(time, "yyyy:MM:dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out var dateTaken))
                dateTaken = File.GetCreationTime(FileName);

            var info = new ExifInformation("f/" + aString,
                                           exif[33434].Value.ToString(),
                                           exif[0x8827].Value.ToString(),
                                           exif[271].Value.ToString().TrimEnd('\0') + " " + exif[272].Value.ToString().TrimEnd('\0'),
                                           dateTaken);

            return info;
        }
    }
}
