﻿using System.Windows;
using System.Windows.Controls;

namespace Parrot.Controls.TileView
{
    /// <summary>Логика взаимодействия для TileWall.xaml</summary>
    public partial class TileWall : UserControl
    {
        public static readonly DependencyProperty TilesSourceProperty = DependencyProperty.Register(
            "TilesSource", typeof(ITilesSource), typeof(TileWall),
            new FrameworkPropertyMetadata(default(ITilesSource)));

        public static readonly DependencyProperty TileTemplateProperty = DependencyProperty.Register(
            "TileTemplate", typeof(DataTemplate), typeof(TileWall), new PropertyMetadata(default(DataTemplate)));

        public TileWall()
        {
            InitializeComponent();
        }

        public DataTemplate TileTemplate
        {
            get => (DataTemplate)GetValue(TileTemplateProperty);
            set => SetValue(TileTemplateProperty, value);
        }

        public ITilesSource TilesSource
        {
            get => (ITilesSource)GetValue(TilesSourceProperty);
            set => SetValue(TilesSourceProperty, value);
        }

        private void Tiles_OnSizeChanged(object Sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Height != e.NewSize.Height && e.PreviousSize.Height > 0)
                Scroller.ScrollToVerticalOffset((e.NewSize.Height - Scroller.ViewportHeight)
                                                * Scroller.ContentVerticalOffset
                                                / (e.PreviousSize.Height - Scroller.ViewportHeight));
        }
    }
}
