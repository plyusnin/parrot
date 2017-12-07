using System.IO;
using System.Windows.Media.Imaging;

namespace Parrot.Viewer.ViewModels.Tiles
{
    public class TileViewModel
    {
        public TileViewModel(Stream ImageStream)
        {
            Image = new BitmapImage();
            Image.BeginInit();
            Image.StreamSource = ImageStream;
            Image.CacheOption = BitmapCacheOption.OnLoad;
            Image.EndInit();
        }

        public BitmapImage Image { get; }
    }
}
