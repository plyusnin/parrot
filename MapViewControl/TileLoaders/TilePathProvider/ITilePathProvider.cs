namespace MapVisualization.TileLoaders.TilePathProvider
{
    public interface ITilePathProvider
    {
        string GetLocalPath(int x, int y, int zoom);
        string GetWebPath(int x, int y, int zoom);
    }
}
