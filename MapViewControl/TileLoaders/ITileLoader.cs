namespace MapVisualization.TileLoaders
{
    /// <summary>Интерфейс загрузчика тайлов карты</summary>
    public interface ITileLoader
    {
        /// <summary>Загружает тайл с указанными индексами</summary>
        /// <param name="x">Горизонтальный индекс</param>
        /// <param name="y">Вертикальный индекс</param>
        /// <param name="zoom">Уровень масштабирования</param>
        ITileLoadingContext GetTile(int x, int y, int zoom);
    }
}
