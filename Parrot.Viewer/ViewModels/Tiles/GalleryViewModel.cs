using Parrot.Viewer.GallerySources;
using ReactiveUI;

namespace Parrot.Viewer.ViewModels.Tiles
{
    public class GalleryViewModel : ReactiveObject
    {
        public GalleryViewModel(IGallerySource Source)
        {
            Tiles = Source.Photos
                          .CreateDerivedCollection(f => new TileViewModel(f.Content));
        }

        public IReactiveDerivedList<TileViewModel> Tiles { get; }
    }
}
