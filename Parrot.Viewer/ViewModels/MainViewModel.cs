using System.Reactive;
using System.Reactive.Linq;
using Parrot.Viewer.Albums;
using Parrot.Viewer.GallerySources;
using Parrot.Viewer.GallerySources.Database;
using Parrot.Viewer.UserInteractions;
using Parrot.Viewer.ViewModels.Single;
using Parrot.Viewer.ViewModels.Tiles;
using ReactiveUI;

namespace Parrot.Viewer.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private bool _fullScreenMode;
        private SingleViewModel _single;

        public MainViewModel()
        {
            Interactions.SingleView.RegisterHandler(c => OpenViewer(c));

            var directory = App.Arguments[0];

            IGallerySource source =
                new DatabaseGallerySource(
                    new FolderGallerySource(directory));
            Gallery = new GalleryViewModel(new FolderAlbum(source, directory));

            CloseViewer = ReactiveCommand.CreateFromTask(async () =>
                                                         {
                                                             await Interactions.CloseViewer.Handle(Unit.Default);
                                                             Single?.Dispose();
                                                             Single = null;
                                                         },
                                                         this.WhenAnyValue(x => x.Single)
                                                             .Select(s => s != null));

            SwitchFullScreen = ReactiveCommand.Create(() => FullScreenMode = !FullScreenMode);
        }

        public ReactiveCommand<Unit, Unit> CloseViewer { get; }
        public ReactiveCommand<Unit, bool> SwitchFullScreen { get; }

        public GalleryViewModel Gallery { get; }

        public SingleViewModel Single
        {
            get => _single;
            set => this.RaiseAndSetIfChanged(ref _single, value);
        }

        public bool FullScreenMode
        {
            get => _fullScreenMode;
            set => this.RaiseAndSetIfChanged(ref _fullScreenMode, value);
        }

        private void OpenViewer(InteractionContext<ViewAlbumRequest, Unit> Context)
        {
            Single = new SingleViewModel(Context.Input.Album, Context.Input.Index);
            Context.SetOutput(Unit.Default);
        }
    }
}
