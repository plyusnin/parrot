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
using ReactiveUI.Legacy;

namespace Parrot.Viewer.ViewModels.Map
{
    public class MapViewModel : ReactiveObject
    {
        private readonly IGallery _gallery;
        private readonly IGeoIndexer _geoIndexer;
        private readonly ReactiveList<GeoStack> _visibleStacks;

        private EarthPoint _mapCenter;

        private EarthArea _visibleArea;
        private int _zoomLevel = 14;

        public MapViewModel(IGallery Gallery, IGeoIndexer GeoIndexer)
        {
            _gallery = Gallery;
            _geoIndexer = GeoIndexer;
            TileLoader = new WebTileLoader(OsmTilePathProviders.Voyager);

            _visibleStacks = new ReactiveList<GeoStack>();
            MapElements = _visibleStacks.CreateDerivedCollection(CreateMapElement);
            this.WhenAnyValue(x => x.ZoomLevel,
                              x => x.VisibleArea,
                              (zoom, area) => new { zoom = zoom + 1, area })
                .Select(x => new
                             {
                                 x.zoom,
                                 x1 = _geoIndexer.GetX(x.area.MostWesternLongitude, x.zoom),
                                 x2 = _geoIndexer.GetX(x.area.MostEasternLongitude, x.zoom),
                                 y1 = _geoIndexer.GetY(x.area.MostNorthenLatitude, x.zoom),
                                 y2 = _geoIndexer.GetY(x.area.MostSouthernLatitude, x.zoom)
                             })
                .DistinctUntilChanged()
                .Select(r => _gallery.GetPhotosOnMap(r.zoom, r.x1, r.x2, r.y1, r.y2))
                .Subscribe(SynchronizeMapElements);
        }

        public ITileLoader TileLoader { get; }

        public IReactiveDerivedList<MapElement> MapElements { get; }

        public EarthPoint MapCenter
        {
            get => _mapCenter;
            set => this.RaiseAndSetIfChanged(ref _mapCenter, value);
        }

        public int ZoomLevel
        {
            get => _zoomLevel;
            set => this.RaiseAndSetIfChanged(ref _zoomLevel, value);
        }

        public EarthArea VisibleArea
        {
            get => _visibleArea;
            set => this.RaiseAndSetIfChanged(ref _visibleArea, value);
        }

        private void SynchronizeMapElements(IList<GeoStack> Elements)
        {
            foreach (var element in _visibleStacks.ToList())
            {
                var comparer = GeoStack.ValueComparer;
                var elementInNew = Elements.FirstOrDefault(e => comparer.Equals(e, element));

                if (elementInNew == null)
                    _visibleStacks.Remove(element);
                else
                    Elements.Remove(elementInNew);
            }
            foreach (var element in Elements)
                _visibleStacks.Add(element);
        }

        private MapElement CreateMapElement(GeoStack Stack)
        {
            var position = new EarthPoint(new Degree(Stack.Points.Average(p => p.Latitude)),
                                          new Degree(Stack.Points.Average(p => p.Longitude)));
            return new PhotoMapElement(position, Stack.Points.Count, _gallery.OpenThumbnail(Stack.LastPhoto));
        }
    }
}
