using System.Drawing;

namespace ImageAIDetector.Utils
{
    //拼图任务
    public class DownloadTask
    {
        //任务的瓦片数据
        public List<TileInfo> Tiles { get; set; } = new List<TileInfo>();
        // 任务的拼图数据
        public byte[]? TaskData { get; set; }
        public string TaskId { get; }
        public Rectangle? Result { get;set; }

        public DownloadTask()
        {
            TaskId = Guid.NewGuid().ToString("N");
        }
    }
}
