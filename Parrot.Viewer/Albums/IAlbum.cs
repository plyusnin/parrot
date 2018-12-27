using Parrot.Viewer.GallerySources;
using ReactiveUI;
using ReactiveUI.Legacy;

namespace Parrot.Viewer.Albums
{
    public interface IAlbum
    {
        IReactiveDerivedList<IPhotoEntity> Photos { get; }
    }
}
