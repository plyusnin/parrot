using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Parrot.Controls.TileView;
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
        private readonly ObservableAsPropertyHelper<ITilesSource> _tiles;

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
                .Select(a => a.Photos)
                .Select(c => new TilesSource(c))
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

        public ITilesSource Tiles => _tiles.Value;

        public void Dispose()
        {
            _disposeOnExit?.Dispose();
        }

        public class TilesSource : ITilesSource
        {
            private readonly IReactiveDerivedList<IPhotoEntity> _source;

            public TilesSource(IReactiveDerivedList<IPhotoEntity> Source)
            {
                _source = Source;
            }

            public IList<ITileViewModel> GetTiles(int StartIndex, int Count)
            {
                return _source.Select((t, i) => new { index = i, tile = t })
                              .Skip(StartIndex)
                              .Take(Count)
                              .Select(x => (ITileViewModel)new ViewModelAdapter(x.index, x.tile))
                              .ToList();
            }

            public class ViewModelAdapter : ITileViewModel
            {
                private readonly BitmapImage _imageSource;
                private readonly IPhotoEntity _photo;

                public ViewModelAdapter(int Index, IPhotoEntity Photo)
                {
                    this.Index = Index;
                    _photo     = Photo;

                    var image = new BitmapImage();

                    image.BeginInit();
                    var thumbnailStream = _photo.OpenThumbnail();
                    image.DownloadCompleted += (s, e) => thumbnailStream.Dispose();
                    image.StreamSource      =  thumbnailStream;
                    image.EndInit();

                    _imageSource = image;
                }

                public int Index { get; }

                public ImageSource ImageSource => _imageSource;
            }
        }
    }
}
