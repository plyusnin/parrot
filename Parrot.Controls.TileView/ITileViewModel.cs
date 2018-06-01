using System.Windows.Media;

namespace Parrot.Controls.TileView
{
    public interface ITileViewModel
    {
        int Index { get; }
        ImageSource ImageSource { get; }
    }
}
