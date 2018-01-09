namespace MapVisualization.TileLoaders
{
    /// <summary>��������� ���������� ������ �����</summary>
    public interface ITileLoader
    {
        /// <summary>��������� ���� � ���������� ���������</summary>
        /// <param name="x">�������������� ������</param>
        /// <param name="y">������������ ������</param>
        /// <param name="zoom">������� ���������������</param>
        ITileLoadingContext GetTile(int x, int y, int zoom);
    }
}
