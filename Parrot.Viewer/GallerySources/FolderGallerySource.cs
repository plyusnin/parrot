using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Parrot.Viewer.GallerySources.Database;
using ReactiveUI;

namespace Parrot.Viewer.GallerySources
{
    public class FolderGallerySource : IFileGallerySource, IDisposable
    {
        private readonly CompositeDisposable _disposeOnExit = new CompositeDisposable();

        private readonly string _root;

        public FolderGallerySource(string Root)
        {
            _root = Root;
            Photos = new ReactiveList<FilePhotoRecord>();

            Directory.EnumerateFiles(_root, "*.jpg", SearchOption.AllDirectories)
                     .ToObservable()
                     //.SubscribeOn(TaskPoolScheduler.Default)
                     .Select(OpenPhoto)
                     .Subscribe(Photos.Add)
                     .DisposeWith(_disposeOnExit);
        }

        public void Dispose() { _disposeOnExit.Dispose(); }

        public IReactiveList<FilePhotoRecord> Photos { get; }

        private FilePhotoRecord OpenPhoto(string File) { return new FilePhotoRecord(File); }
    }
}
