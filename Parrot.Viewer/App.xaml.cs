using System.Windows;

namespace Parrot.Viewer
{
    /// <summary>Логика взаимодействия для App.xaml</summary>
    public partial class App : Application
    {
        public static string[] Arguments { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            Arguments = e.Args;
            base.OnStartup(e);
        }
    }
}
