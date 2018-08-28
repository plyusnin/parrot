using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Parrot.Controls.TileView;
using Parrot.Viewer.GallerySources;
using ReactiveUI;

namespace Parrot.Viewer.ViewModels.Tiles
{
    public class GalleryViewModel : ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable _disposeOnExit = new CompositeDisposable();
        private readonly IGallery _gallery;
        private readonly ObservableAsPropertyHelper<ITilesSource> _tiles;

        private string _directory;
        private int _selectedPhotoIndex;

        public GalleryViewModel(IGallery Gallery)
        {
            _gallery = Gallery;
            Tiles = new GalleryTilesSource(_gallery);
        }

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

        public ITilesSource Tiles { get; }

        public void Dispose()
        {
            _disposeOnExit?.Dispose();
        }
    }

    public class GalleryTilesSource : ITilesSource
    {
        private readonly IGallery _gallery;

        public GalleryTilesSource(IGallery Gallery)
        {
            _gallery = Gallery;
        }

        public IList<ITileViewModel> GetTiles(int StartIndex, int Count)
        {
            return _gallery.All(StartIndex, Count)
                           .Select((tile, i) => (ITileViewModel)new ViewModelAdapter(StartIndex + i, tile, _gallery.OpenThumbnail(tile)))
                           .ToList();
            //return Enumerable.Range(StartIndex, Count)
            //               .Select((tile, i) => (ITileViewModel)new ViewModelAdapter(StartIndex + i, new PhotoEntity("sasdf", null), null))
            //               .ToList();
        }

        public int Count => _gallery.Count;

        public class ViewModelAdapter : ITileViewModel
        {
            private readonly BitmapImage _imageSource;
            private readonly IPhotoEntity _photo;

            public ViewModelAdapter(int Index, IPhotoEntity Photo, Stream ThumbnailStream)
            {
                this.Index = Index;
                this.ThumbnailStream = ThumbnailStream;
                _photo = Photo;

                //image.BeginInit();
                //image.DownloadCompleted += (s, e) => ThumbnailStream.Dispose();
                //image.StreamSource = ThumbnailStream;
                //image.EndInit();
            }

            public int Index { get; }
            public Stream ThumbnailStream { get; }

            //public ImageSource ImageSource => _imageSource;

            public override string ToString()
            {
                return $"{Index.ToString().PadLeft(4)}  {Path.GetFileNameWithoutExtension(_photo.FileName)}";
            }
        }
    }
}
