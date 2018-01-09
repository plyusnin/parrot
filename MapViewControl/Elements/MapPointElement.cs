using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Geographics;

namespace MapVisualization.Elements
{
    public abstract class MapPointElement : MapElement
    {
        private const double HorizontalStackPadding = 4;
        private const double VerticalStackPadding = 2;
        protected static readonly SolidColorBrush TextBackgroundBrush = new SolidColorBrush(Colors.White) { Opacity = 0.8 };
        protected static readonly Pen TextBoxStrokePen = new Pen(Brushes.DimGray, 1);

        private EarthPoint _position;
        public MapPointElement(EarthPoint Position) { _position = Position; }

        public EarthPoint Position
        {
            get { return _position; }
            set
            {
                _position = value;
                OnPositionChanged();
            }
        }

        /// <summary>Возникает при изменении позиции точки</summary>
        protected virtual void OnPositionChanged() { RequestChangeVisual(); }

        protected abstract void DrawPointElement(DrawingContext dc, int Zoom);

        protected override void Draw(DrawingContext dc, int Zoom)
        {
            var elementPoint = Projector.Project(Position, Zoom);
            dc.PushTransform(new TranslateTransform(elementPoint.X, elementPoint.Y));
            DrawPointElement(dc, Zoom);
            dc.Pop();
        }

        protected static void PrintStack(DrawingContext dc, params FormattedText[] labels) { PrintStack(dc, (IList<FormattedText>)labels); }

        protected static void PrintStack(DrawingContext dc, IList<FormattedText> labels)
        {
            dc.DrawRoundedRectangle(TextBackgroundBrush, TextBoxStrokePen,
                                    new Rect(-HorizontalStackPadding - TextBoxStrokePen.Thickness * 0.5,
                                             -TextBoxStrokePen.Thickness * 0.5 - VerticalStackPadding,
                                             Math.Round(labels.Max(l => l.Width)) + 2 * HorizontalStackPadding,
                                             Math.Round(labels.Sum(l => l.Height + 1)) + 2 * VerticalStackPadding),
                                    2, 2);
            double yOffset = 0;
            foreach (var label in labels)
            {
                dc.DrawText(label, new Point(0, yOffset));
                yOffset += label.Height + 1;
            }
        }

        public override bool TestVisual(EarthArea VisibleArea) { return Position.IsInArea(VisibleArea); }
    }
}
