using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Parrot.Controls.TileView
{
    public class TileViewModel
    {
        public TileViewModel(int Index, string Name, ImageSource Thumbnail)
        {
            this.Index = Index;
            this.Name = Name;
            this.Thumbnail = Thumbnail;
        }

        public int Index { get; }
        public string Name { get; }
        public ImageSource Thumbnail { get; }
    }

    public class TileViewModelFactory
    {
        public TileViewModel CreateInstance(ITileViewModel Element)
        {
            var imageSource = new BitmapImage();
            using (var stream = Element.OpenThumbnail())
            {
                imageSource.BeginInit();
                imageSource.StreamSource = stream;
                imageSource.DownloadCompleted += (s, e) => imageSource.Freeze();
                imageSource.EndInit();
            }
            return new TileViewModel(Element.Index, Path.GetFileNameWithoutExtension(Element.Name), imageSource);
        }
    }
}
