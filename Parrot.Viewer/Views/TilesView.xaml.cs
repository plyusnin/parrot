using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Parrot.Viewer.Views
{
    /// <summary>Логика взаимодействия для TilesView.xaml</summary>
    public partial class TilesView : UserControl
    {
        public TilesView()
        {
            InitializeComponent();

            Ellipse ellipse = new Ellipse
            {
                Width = 3,
                Height = 3,
                Fill = Brushes.Chartreuse
            };

            Canvas.SetLeft(ellipse, 50);
            Canvas.SetTop(ellipse, 50);
            canvas.Children.Add(ellipse);
        }
    }
}
