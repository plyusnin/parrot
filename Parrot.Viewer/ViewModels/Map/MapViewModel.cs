using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Geographics;
using MapVisualization.Elements;
using MapVisualization.TileLoaders;
using MapVisualization.TileLoaders.TilePathProvider;
using Parrot.Viewer.GallerySources;
using ReactiveUI;

namespace Parrot.Viewer.ViewModels.Map
{
    public class MapViewModel : ReactiveObject
    {
        private readonly IGallerySource _gallery;
        private int _zoomLevel = 14;

        public MapViewModel(IGallerySource Gallery)
        {
            _gallery   = Gallery;
            TileLoader = new WebTileLoader(OsmTilePathProviders.Retina);

            MapElements = new ReactiveList<MapElement>();
            this.WhenAnyValue(x => x.ZoomLevel)
                .Select(StackPhotos)
                .Subscribe(el =>
                           {
                               MapElements.Clear();
                               MapElements.AddRange(el);
                           });
        }

        public ITileLoader TileLoader { get; }

        public ReactiveList<MapElement> MapElements { get; }

        public int ZoomLevel
        {
            get => _zoomLevel;
            set => this.RaiseAndSetIfChanged(ref _zoomLevel, value);
        }

        private IList<MapElement> StackPhotos(int Zoom)
        {
            var factor = (1 << (25 - Zoom)) - 1;

            int Round(double value) { return (int)value & ~factor; }

            if (Zoom == 0)
                return new List<MapElement>();

            return _gallery.Photos
                           .Where(f => f.Exif.Gps != null)
                           .GroupBy(f => Tuple.Create(Round(Mercator.LatitudeToY(f.Exif.Gps.Value.Latitude)),
                                                      Round(Mercator.LongitudeToX(f.Exif.Gps.Value.Longitude))))
                           .Select(g => CreateMapElement(g.ToList()))
                           .ToList();
        }

        private MapElement CreateMapElement(ICollection<IPhotoEntity> Photos)
        {
            var position = new EarthPoint(new Degree(Photos.Average(f => f.Exif.Gps.Value.Latitude)),
                                          new Degree(Photos.Average(f => f.Exif.Gps.Value.Longitude)));
            return new PhotoMapElement(position, Photos.Last(), Photos.Count);
        }
    }
}
