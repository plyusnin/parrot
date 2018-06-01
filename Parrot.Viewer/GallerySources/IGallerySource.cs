using ReactiveUI;

namespace Parrot.Viewer.GallerySources
{
    public interface IGallerySource
    {
        IReactiveList<IPhotoEntity> Photos { get; }
    }
}
