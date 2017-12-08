using System;
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
        private readonly ObservableAsPropertyHelper<ImageSource> _picturePreview;

        private int _index;

        public SingleViewModel(IAlbum Album, int InputIndex)
        {
            Index = InputIndex;

            this.WhenAnyValue(x => x.Index)
                .Select(i => Album.Photos[i])
                .SelectMany(f => Observable.Start(() => CreateImageSource(f))
                                           .StartWith((ImageSource)null))
                .ObserveOnDispatcher()
                .ToProperty(this, x => x.Picture, out _picture)
                .DisposeWith(_disposeOnExit);

            this.WhenAnyValue(x => x.Index)
                .Select(i => Album.Photos[i])
                .Select(CreatePreviewImageSource)
                .ToProperty(this, x => x.PicturePreview, out _picturePreview)
                .DisposeWith(_disposeOnExit);

            Next = ReactiveCommand.Create(() => Index += 1, this.WhenAnyValue(x => x.Index).Select(i => i < Album.Photos.Count - 1));
            Previous = ReactiveCommand.Create(() => Index -= 1, this.WhenAnyValue(x => x.Index).Select(i => i > 0));
        }

        public ReactiveCommand<Unit, int> Next { get; }
        public ReactiveCommand<Unit, int> Previous { get; }

        public ImageSource Picture => _picture.Value;
        public ImageSource PicturePreview => _picturePreview.Value;

        public int Index
        {
            get => _index;
            set => this.RaiseAndSetIfChanged(ref _index, value);
        }

        public void Dispose() { _disposeOnExit?.Dispose(); }

        private ImageSource CreatePreviewImageSource(IPhotoEntity Photo)
        {
            var bitmapimage = new BitmapImage();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = Photo.Thumbnail;
            bitmapimage.CacheOption = BitmapCacheOption.OnDemand;
            bitmapimage.EndInit();
            return bitmapimage;
        }

        private ImageSource CreateImageSource(IPhotoEntity Photo)
        {
            var decoder = new JpegBitmapDecoder(new Uri(Photo.FileName), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            decoder.Frames[0].Freeze();
            return decoder.Frames[0];
        }
    }
}
