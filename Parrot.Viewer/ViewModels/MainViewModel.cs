﻿using System.Reactive;
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
        private SingleViewModel _single;

        public MainViewModel()
        {
            Interactions.SingleView.RegisterHandler(c => OpenViewer(c));

            IGallerySource source =
                new DatabaseGallerySource(
                    new FolderGallerySource(@"C:\Users\plyusnin\Desktop\Photos"));
            Gallery = new GalleryViewModel(new FolderAlbum(source, @"C:\Users\plyusnin\Desktop\Photos"));

            CloseViewer = ReactiveCommand.CreateFromTask(async () =>
                                                         {
                                                             await Interactions.CloseViewer.Handle(Unit.Default);
                                                             Single?.Dispose();
                                                             Single = null;
                                                         },
                                                         this.WhenAnyValue(x => x.Single)
                                                             .Select(s => s != null));
        }

        public ReactiveCommand<Unit, Unit> CloseViewer { get; }

        public GalleryViewModel Gallery { get; }

        public SingleViewModel Single
        {
            get => _single;
            set => this.RaiseAndSetIfChanged(ref _single, value);
        }

        private void OpenViewer(InteractionContext<ViewAlbumRequest, Unit> Context)
        {
            Single = new SingleViewModel(Context.Input.Album, Context.Input.Index);
            Context.SetOutput(Unit.Default);
        }
    }
}
