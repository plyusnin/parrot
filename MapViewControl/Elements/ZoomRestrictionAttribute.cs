using System;

namespace MapVisualization.Elements
{
    public class ZoomRestrictionAttribute : Attribute
    {
        public ZoomRestrictionAttribute(int MinZoomLevel) { this.MinZoomLevel = MinZoomLevel; }
        public int MinZoomLevel { get; private set; }
    }
}
