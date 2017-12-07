using Parrot.Viewer.GallerySources;
using Parrot.Viewer.ViewModels.Tiles;
using ReactiveUI;

namespace Parrot.Viewer.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel()
        {
            IGallerySource source = new FolderGallerySource(@"C:\Users\plyusnin\Desktop\Photos");
            Gallery = new GalleryViewModel(source);
        }

        public GalleryViewModel Gallery { get; }
    }
}
