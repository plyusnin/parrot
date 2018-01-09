using ReactiveUI;

namespace Parrot.Viewer.GallerySources
{
    public interface IFileGallerySource
    {
        IReactiveList<FilePhotoRecord> Photos { get; }
    }

    public class FilePhotoRecord
    {
        public FilePhotoRecord(string FileName) { this.FileName = FileName; }
        public string FileName { get; }
    }
}
