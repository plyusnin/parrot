using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Parrot.Viewer.UserInteractions;
using Parrot.Viewer.ViewModels;
using ReactiveUI;

namespace Parrot.Viewer
{
    /// <summary>Логика взаимодействия для MainWindow.xaml</summary>
    public partial class MainWindow : Window
    {
        private readonly SemaphoreSlim _viewerClosingSemaphore = new SemaphoreSlim(0);

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            Interactions.CloseViewer.RegisterHandler(CloseViewerHandler);
        }

        public event EventHandler StartViewerClosingAnimation;

        private async Task CloseViewerHandler(InteractionContext<Unit, Unit> Context)
        {
            StartViewerClosingAnimation?.Invoke(this, EventArgs.Empty);
            await _viewerClosingSemaphore.WaitAsync();
            Context.SetOutput(Unit.Default);
        }

        private void CloseViewerStoryboard_OnCompleted(object Sender, EventArgs E) { _viewerClosingSemaphore.Release(); }
    }
}
