using System;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Parrot.Viewer.Albums;
using Parrot.Viewer.GallerySources;
using ReactiveUI;

namespace Parrot.Viewer.ViewModels.Single
{
    public class SingleViewModel : ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable _disposeOnExit = new CompositeDisposable();
        private readonly ObservableAsPropertyHelper<ImageSource> _picture;

        private int _index;

        public SingleViewModel(IAlbum Album, int InputIndex)
        {
            Index = InputIndex;

            this.WhenAnyValue(x => x.Index)
                .Select(i => Album.Photos[i])
                .Select(CreateImageSource)
                .ToProperty(this, x => x.Picture, out _picture)
                .DisposeWith(_disposeOnExit);

            Next = ReactiveCommand.Create(() => Index += 1,
                                          this.WhenAnyValue(x => x.Index)
                                              .Select(i => i > 0));
            Previous = ReactiveCommand.Create(() => Index -= 1,
                                              this.WhenAnyValue(x => x.Index)
                                                  .Select(i => i < Album.Photos.Count - 1));
        }

        public ReactiveCommand<Unit, int> Next { get; }
        public ReactiveCommand<Unit, int> Previous { get; }

        public ImageSource Picture => _picture.Value;

        public int Index
        {
            get => _index;
            set => this.RaiseAndSetIfChanged(ref _index, value);
        }

        public void Dispose() { _disposeOnExit?.Dispose(); }

        private ImageSource CreateImageSource(IPhotoEntity Photo)
        {
            var bitmapimage = new BitmapImage();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = File.OpenRead(Photo.FileName);
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();
            return bitmapimage;
        }
    }
}
