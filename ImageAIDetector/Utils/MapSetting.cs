namespace ImageAIDetector.Utils
{
    public class MapSetting
    {
        public IList<MapLod> lods { get; set; }
    }

    /// <summary>
    /// 地图切片比例尺
    /// </summary>
    public class MapLod
    {
        public int level { get; set; }
        public double resolution { get; set; }
        public int scale { get; set; }
    }
    /// <summary>
    /// 地图瓦片信息
    /// </summary>
    public class TileInfo
    {
        public int col { get; set; }
        public int row { get; set; }
        public byte[] data { get; set; } 
    }

}
