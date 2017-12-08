using System.Reactive;
using Parrot.Viewer.Albums;
using ReactiveUI;

namespace Parrot.Viewer.UserInteractions
{
    public static class Interactions
    {
        public static Interaction<ViewAlbumRequest, Unit> SingleView { get; } = new Interaction<ViewAlbumRequest, Unit>();
        public static Interaction<Unit, Unit> CloseViewer { get; } = new Interaction<Unit, Unit>();
    }

    public class ViewAlbumRequest
    {
        public ViewAlbumRequest(IAlbum Album, int Index)
        {
            this.Album = Album;
            this.Index = Index;
        }

        public IAlbum Album { get; }
        public int Index { get; }
    }
}
