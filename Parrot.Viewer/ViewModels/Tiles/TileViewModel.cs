using System.IO;
using System.Windows.Media.Imaging;
using Parrot.Viewer.GallerySources;

namespace Parrot.Viewer.ViewModels.Tiles
{
    public class TileViewModel
    {
        public TileViewModel(Stream ThumbnailStream, ExifInformation Exif)
        {
            this.Exif = Exif;
            Thumbnail = new BitmapImage();
            Thumbnail.BeginInit();
            Thumbnail.StreamSource = ThumbnailStream;
            Thumbnail.CacheOption = BitmapCacheOption.OnLoad;
            Thumbnail.EndInit();
        }

        public BitmapImage Thumbnail { get; }
        public ExifInformation Exif { get; }
    }
}
