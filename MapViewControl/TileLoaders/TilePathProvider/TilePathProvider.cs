using System;
using System.IO;
using System.Reflection;

namespace MapVisualization.TileLoaders.TilePathProvider
{
    public class TilePathProvider : ITilePathProvider
    {
        private static readonly string _tilesCacheRoot =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "Saut", Assembly.GetExecutingAssembly().GetName().Name);

        private readonly string _cacheDirectory;
        private readonly string _webPath;

        public TilePathProvider(string CacheDirectory, string WebPath)
        {
            _cacheDirectory = CacheDirectory;
            _webPath = WebPath;
        }

        public string GetLocalPath(int x, int y, int zoom)
        {
            return Path.Combine(_tilesCacheRoot, _cacheDirectory,
                                zoom.ToString(), String.Format("{0}.{1}.png", x, y));
        }

        public string GetWebPath(int x, int y, int zoom)
        {
            return _webPath.Replace("{x}", x.ToString())
                           .Replace("{y}", y.ToString())
                           .Replace("{zoom}", zoom.ToString());
        }
    }
}
