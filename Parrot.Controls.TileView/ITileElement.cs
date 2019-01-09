using System.IO;

namespace Parrot.Controls.TileView
{
    public interface ITileElement
    {
        int Index { get; }
        string Name { get; }
        Stream OpenThumbnail();
    }
}
