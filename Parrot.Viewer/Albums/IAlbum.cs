using Parrot.Viewer.GallerySources;
using ReactiveUI;

namespace Parrot.Viewer.Albums
{
    public interface IAlbum
    {
        IReactiveDerivedList<IPhotoEntity> Photos { get; }
    }
}
