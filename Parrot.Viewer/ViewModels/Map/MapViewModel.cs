using MapVisualization.Elements;
using MapVisualization.TileLoaders;
using MapVisualization.TileLoaders.TilePathProvider;
using Parrot.Viewer.GallerySources;
using ReactiveUI;

namespace Parrot.Viewer.ViewModels.Map
{
    public class MapViewModel : ReactiveObject
    {
        public MapViewModel(IGallerySource Gallery)
        {
            TileLoader = new WebTileLoader(OsmTilePathProviders.Retina);

            MapElements = Gallery.Photos
                                 .CreateDerivedCollection(CreateMapElement, f => f.Exif.Gps != null);
        }

        public ITileLoader TileLoader { get; }

        public IReactiveDerivedList<MapElement> MapElements { get; }

        private MapElement CreateMapElement(IPhotoEntity Photo) { return new PhotoMapElement(Photo); }
    }
}
