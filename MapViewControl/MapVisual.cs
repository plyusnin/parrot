using System.Windows.Media;
using MapVisualization.Elements;

namespace MapVisualization
{
    public class MapVisual : DrawingVisual
    {
        public MapElement Element { get; private set; }

        public MapVisual(MapElement Element, int ZIndex = 0)
        {
            this.Element = Element;
            this.ZIndex = ZIndex;
        }

        /// <summary>Z-������ ����������� ��������</summary>
        public readonly int ZIndex;
    }
}
