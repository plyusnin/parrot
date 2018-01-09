﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using Parrot.Viewer.GallerySources;
using Parrot.Viewer.GallerySources.Database;
using Parrot.Viewer.ViewModels.Single;
using Parrot.Viewer.ViewModels.Tiles;
using ReactiveUI;

namespace Parrot.Viewer.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<SingleViewModel> _single;

        private bool _fullScreenMode;

        public MainViewModel()
        {
            var directory = App.Arguments[0];

            IGallerySource source =
                new DatabaseGallerySource(
                    new FolderGallerySource(directory));

            Gallery = new GalleryViewModel(source) { Directory = directory };

            Gallery.WhenAnyValue(x => x.Album)
                   .Select(a => new SingleViewModel(a, 0))
                   .ToProperty(this, x => x.Single, out _single);

            Gallery.WhenAnyValue(x => x.SelectedPhotoIndex)
                   .Subscribe(i => Single.Index = i);

            SwitchFullScreen = ReactiveCommand.Create(() => FullScreenMode = !FullScreenMode);
        }

        public SingleViewModel Single => _single.Value;

        public ReactiveCommand<Unit, bool> SwitchFullScreen { get; }

        public GalleryViewModel Gallery { get; }

        public bool FullScreenMode
        {
            get => _fullScreenMode;
            set => this.RaiseAndSetIfChanged(ref _fullScreenMode, value);
        }
    }
}
