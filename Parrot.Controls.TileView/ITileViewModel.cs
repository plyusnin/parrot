using System.IO;

namespace Parrot.Controls.TileView
{
    public interface ITileViewModel
    {
        int Index { get; }
        string Name { get; }
        Stream OpenThumbnail();
    }
}
