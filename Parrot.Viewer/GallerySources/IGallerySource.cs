using ReactiveUI;
using ReactiveUI.Legacy;

namespace Parrot.Viewer.GallerySources
{
    public interface IGallerySource
    {
        IReactiveList<IPhotoEntity> Photos { get; }
    }
}
