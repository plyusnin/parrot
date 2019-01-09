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
        public TileViewModel CreateInstance(ITileElement Element)
        {
            var imageSource = new BitmapImage();
            var ms = new MemoryStream();
            using (var stream = Element.OpenThumbnail())
            {
                stream.CopyTo(ms);
            }
            ms.Seek(0, SeekOrigin.Begin);
            imageSource.BeginInit();
            imageSource.StreamSource = ms;
            imageSource.EndInit();
            imageSource.Freeze();
            return new TileViewModel(Element.Index, Path.GetFileNameWithoutExtension(Element.Name), imageSource);
        }
    }
}
