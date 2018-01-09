using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Parrot.Viewer.Albums;
using Parrot.Viewer.GallerySources;
using ReactiveUI;

namespace Parrot.Viewer.ViewModels.Tiles
{
    public class GalleryViewModel : ReactiveObject, IDisposable
    {
        private readonly ObservableAsPropertyHelper<FolderAlbum> _album;

        private readonly CompositeDisposable _disposeOnExit = new CompositeDisposable();
        private readonly IGallerySource _gallery;
        private readonly ObservableAsPropertyHelper<IReactiveDerivedList<TileViewModel>> _tiles;

        private string _directory;
        private int _selectedPhotoIndex;

        public GalleryViewModel(IGallerySource Gallery)
        {
            _gallery = Gallery;

            this.WhenAnyValue(x => x.Directory)
                .Where(dir => dir != null)
                .Select(dir => new FolderAlbum(_gallery, dir))
                .ToProperty(this, x => x.Album, out _album)
                .DisposeWith(_disposeOnExit);

            this.WhenAnyValue(x => x.Album)
                .Where(a => a != null)
                .Select(a => a.Photos
                              .CreateDerivedCollection(f => new TileViewModel(f.Exif, f.OpenThumbnail()),
                                                       scheduler: DispatcherScheduler.Current))
                .ToProperty(this, x => x.Tiles, out _tiles)
                .DisposeWith(_disposeOnExit);
        }

        public FolderAlbum Album => _album.Value;

        public int SelectedPhotoIndex
        {
            get => _selectedPhotoIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedPhotoIndex, value);
        }

        public string Directory
        {
            get => _directory;
            set => this.RaiseAndSetIfChanged(ref _directory, value);
        }

        public IReactiveDerivedList<TileViewModel> Tiles => _tiles.Value;

        public void Dispose() { _disposeOnExit?.Dispose(); }
    }
}
