using System.Reactive.Concurrency;
using Parrot.Viewer.GallerySources;
using ReactiveUI;

namespace Parrot.Viewer.ViewModels.Tiles
{
    public class GalleryViewModel : ReactiveObject
    {
        public GalleryViewModel(IGallerySource Source)
        {
            Tiles = Source.Photos
                          .CreateDerivedCollection(f => new TileViewModel(f.Thumbnail, f.Exif),
                                                   scheduler: DispatcherScheduler.Current);
        }

        public IReactiveDerivedList<TileViewModel> Tiles { get; }
    }
}
