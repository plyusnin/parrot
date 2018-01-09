using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Parrot.Viewer.Albums;
using Parrot.Viewer.UserInteractions;
using ReactiveUI;

namespace Parrot.Viewer.ViewModels.Tiles
{
    public class GalleryViewModel : ReactiveObject
    {
        private readonly IAlbum _album;

        public GalleryViewModel(IAlbum Album)
        {
            _album = Album;
            Tiles = Album.Photos
                         .CreateDerivedCollection(f => new TileViewModel(f.Exif, f.OpenThumbnail()),
                                                  scheduler: DispatcherScheduler.Current);

            Open = ReactiveCommand.CreateFromTask<TileViewModel, Unit>(OpenViewer);
        }

        public IReactiveDerivedList<TileViewModel> Tiles { get; }
        public ReactiveCommand<TileViewModel, Unit> Open { get; }

        private async Task<Unit> OpenViewer(TileViewModel Tile)
        {
            var index = ((IList<TileViewModel>)Tiles).IndexOf(Tile);
            await Interactions.SingleView.Handle(new ViewAlbumRequest(_album, index));

            return Unit.Default;
        }
    }
}
