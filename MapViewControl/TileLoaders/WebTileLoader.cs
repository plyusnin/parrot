using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;
using MapVisualization.TileLoaders.TilePathProvider;

namespace MapVisualization.TileLoaders
{
    /// <summary>Загрузчик тайлов, обеспечивающий кеширование тайлов в файловой системе</summary>
    public class WebTileLoader : ITileLoader
    {
        private readonly ITilePathProvider _pathProvider;

        public WebTileLoader(ITilePathProvider PathProvider) { _pathProvider = PathProvider; }

        /// <summary>Загружает тайл с указанными индексами</summary>
        /// <param name="x">Горизонтальный индекс</param>
        /// <param name="y">Вертикальный индекс</param>
        /// <param name="zoom">Уровень масштабирования</param>
        /// <returns>ImageSource тайла</returns>
        public ITileLoadingContext GetTile(int x, int y, int zoom)
        {
            var context = new LoadingContext(_pathProvider.GetLocalPath(x, y, zoom),
                                             _pathProvider.GetWebPath(x, y, zoom));
            context.BeginLoading();
            return context;
        }

        private class LoadingContext : ITileLoadingContext
        {
            private readonly string _localPath;
            private readonly string _webPath;
            private WebClient _webClient;

            public LoadingContext(string LocalPath, string WebPath)
            {
                _localPath = LocalPath;
                _webPath = WebPath;
            }

            public bool IsReady { get; private set; }
            public BitmapImage Image { get; private set; }
            public event EventHandler Ready;

            public void Abort()
            {
                if (_webClient != null)
                    _webClient.CancelAsync();
            }

            protected virtual void OnReady()
            {
                EventHandler handler = Ready;
                if (handler != null) handler(this, EventArgs.Empty);
            }

            public async void BeginLoading()
            {
                try
                {
                    if (!File.Exists(_localPath))
                    {
                        using (_webClient = new WebClient())
                        {
                            byte[] tileData = await _webClient.DownloadDataTaskAsync(_webPath);
                            Directory.CreateDirectory(Path.GetDirectoryName(_localPath));
                            File.WriteAllBytes(_localPath, tileData);
                        }
                    }
                    Image = new BitmapImage(new Uri(_localPath));
                    IsReady = true;
                    OnReady();
                }
                catch (Exception e)
                {
                    Debug.Print(" # Load tile error: {0}", e.Message);
                }
            }
        }
    }
}
