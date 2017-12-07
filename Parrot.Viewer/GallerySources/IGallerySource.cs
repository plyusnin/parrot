using System.Collections.Generic;
using System.IO;
using photo.exif;
using ReactiveUI;

namespace Parrot.Viewer.GallerySources
{
    public interface IGallerySource
    {
        IReactiveList<IPhotoEntity> Photos { get; }
    }

    public interface IPhotoEntity
    {
        Stream Content { get; }
    }

    public abstract class PhotoEntityBase : IPhotoEntity
    {
        private readonly Dictionary<int, ExifItem> _exifItems;
        protected PhotoEntityBase(Dictionary<int, ExifItem> ExifItems) { _exifItems = ExifItems; }
        public abstract Stream Content { get; }
    }

    public class FilePhotoEntity : PhotoEntityBase
    {
        private readonly string _path;
        public FilePhotoEntity(Dictionary<int, ExifItem> ExifItems, string Path) : base(ExifItems) { _path = Path; }
        public override Stream Content => File.OpenRead(_path);
    }

    public class MemoryPhotoEntity : PhotoEntityBase
    {
        private readonly byte[] _content;
        public MemoryPhotoEntity(Dictionary<int, ExifItem> ExifItems, byte[] Content) : base(ExifItems) { _content = Content; }
        public override Stream Content => new MemoryStream(_content);
    }
}
