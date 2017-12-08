using Parrot.Viewer.GallerySources;
using ReactiveUI;

namespace Parrot.Viewer.Albums
{
    public class FolderAlbum : IAlbum
    {
        public FolderAlbum(IGallerySource Gallery, string Folder)
        {
            Photos = Gallery.Photos
                            .CreateDerivedCollection(x => x, x => x.FileName.StartsWith(Folder));
        }

        public IReactiveDerivedList<IPhotoEntity> Photos { get; }
    }
}
