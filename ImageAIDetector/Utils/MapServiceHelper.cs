using CustomVision;
using Microsoft.AspNetCore.Http.Features;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using FeatureCollection = NetTopologySuite.Features.FeatureCollection;

namespace ImageAIDetector.Utils
{
    public class MapServiceHelper
    {
        private readonly TargetObject _targetObject;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger _logger;
        private List<MapLod> _mapLods = new List<MapLod>();
        private const int DownloadLevel = 8;
        private int _originX = 0;
        private int _originY = 0;
        private int _spatialReferenceWkid = 0;

        public List<DownloadTask> Jobs = new List<DownloadTask>();

        /// <summary>
        /// 拼图方案 4*3 
        /// </summary>
        public const int DrawImageRow = 3;
        public const int DrawImageCol = 4;

        public MapServiceHelper(TargetObject target, IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _targetObject = target;
            _clientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// 检查服务地址是否可用并获取地图服务元数据
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheclAvailable()
        {
            var client = _clientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, _targetObject.mapServiceUrl + "?f=pjson");
            request.Headers.Add("User-Agent", "HttpClientFactory-AI");
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                IConfiguration mapconfig = new ConfigurationBuilder().AddJsonStream(responseStream).Build();
                var valuesSection = mapconfig.GetSection("tileInfo:lods");
                if(valuesSection == null)
                {
                    return false;
                }
                foreach (IConfigurationSection section in valuesSection.GetChildren())
                {
                    var maplod = new MapLod { level = section.GetValue<int>("level"), resolution = section.GetValue<float>("resolution"), scale = section.GetValue<int>("scale") };
                    _mapLods.Add(maplod);
                    _logger.LogInformation($"level={maplod.level},resolution={maplod.resolution},scale={maplod.scale}");
                }

                _originX = mapconfig.GetValue<int>("tileInfo:origin:x");
                _originY = mapconfig.GetValue<int>("tileInfo:origin:y");
                _spatialReferenceWkid = mapconfig.GetValue<int>("spatialReference:wkid");
                _logger.LogInformation($"orgin x = {_originX} , y = {_originY}");
                return true;
            }
            return false;
        }


        
        public int CalcularTaskJobs()
        {
            if(_targetObject != null)
            {
                //获取四个角瓦片索引
                var tileLeftop = CalcularTileIndex(_targetObject.LeftTop.x, _targetObject.LeftTop.y, DownloadLevel);
                var tileRightTop = CalcularTileIndex(_targetObject.RightTop.x, _targetObject.RightTop.y, DownloadLevel);
                var tileLeftBottom = CalcularTileIndex(_targetObject.LeftBottom.x, _targetObject.LeftBottom.y, DownloadLevel);
                var tileRightBottom = CalcularTileIndex(_targetObject.RightBottom.x, _targetObject.RightBottom.y, DownloadLevel);
                var allrows = tileLeftBottom.row - tileLeftop.row + 1;
                var allcols = tileRightBottom.col - tileLeftBottom.col + 1;
                var maxRow = tileRightBottom.row;
                var maxCol = tileRightBottom.col;

                _logger.LogInformation($"cols = {allcols} , row = {allrows} , all tile count = {allcols*allrows}");
                
                //计算出纵向下载任务的次数
                var rowTasks = (int)Math.Ceiling(allrows / (double)DrawImageRow);
                
                //计算出横向下载任务的次数
                var colTasks = (int)Math.Ceiling(allcols / (double)DrawImageCol);

                //任务创建完成,每个任务最多DrawImageX* DrawImageY 个瓦片，部分任务可能不足8个瓦片
                for (int j = 0; j < rowTasks; j++)
                {
                    var startRow = tileLeftop.row + j * DrawImageRow;
                    for (int i = 0; i < colTasks; i++)
                    {
                        var startCol = tileLeftop.col + i * DrawImageCol;
                        var item = CreateTask(startRow, startCol, maxRow, maxCol);
                        if(item.Tiles.Count > DrawImageRow* DrawImageCol / 2)
                        {
                            Jobs.Add(item);
                        }
                    }
                    
                }

                return Jobs.Count;

            }
            return 0;

        }
        /// <summary>
        /// 创建下载任务
        /// </summary>
        /// <param name="rowStart"></param>
        /// <param name="colStart"></param>
        /// <param name="maxRow"></param>
        /// <param name="maxCol"></param>
        /// <returns></returns>
        private DownloadTask CreateTask(int rowStart, int colStart, int maxRow, int maxCol)
        {
            var taskdownload = new DownloadTask();
            for (int i = 0; i < DrawImageRow; i++)
            {
                for (int j = 0; j < DrawImageCol; j++)
                {
                    if (rowStart + i > maxRow || colStart + j > maxCol)
                    {
                        break;
                    }
                    var taskTile = new TileInfo { row = rowStart + i, col = colStart + j, level = DownloadLevel };
                    taskdownload.Tiles.Add(taskTile);
                }

            }
            return taskdownload;
        }

        public async Task<FeatureCollection> ProcessAllJobs(IDetectEngine detectEngine)
        {
            var result = new FeatureCollection();
            await Task.WhenAll(Jobs.Select(async job => { 
                
                var ret = await ProcessJob(job,detectEngine);
                if (ret != null)
                {
                    foreach (var item in ret)
                    {
                        job.Result.Add(ConvertToFeature(item, job.Tiles.Min(p => p.row), job.Tiles.Min(p => p.col)));
                    }
                    
                }
                
            }));
            
            foreach(var job in Jobs)
            {
                if(job.Result != null)
                {
                    foreach(var item in job.Result)
                    {
                        result.Add(item);
                    }
                }
            }
            return result;
        }

        private Feature ConvertToFeature(IdentityRect result,int StartTileRow,int StartTileCol)
        {
            List<Coordinate> coordinates = new List<Coordinate>();
            coordinates.Add(ConvertPixelToCoordinate(result.rectangle.Left, result.rectangle.Top, StartTileCol, StartTileRow));
            coordinates.Add(ConvertPixelToCoordinate(result.rectangle.Right, result.rectangle.Top, StartTileCol, StartTileRow));
            coordinates.Add(ConvertPixelToCoordinate(result.rectangle.Right, result.rectangle.Bottom, StartTileCol, StartTileRow));
            coordinates.Add(ConvertPixelToCoordinate(result.rectangle.Left, result.rectangle.Bottom, StartTileCol, StartTileRow));
            coordinates.Add(ConvertPixelToCoordinate(result.rectangle.Left, result.rectangle.Top, StartTileCol, StartTileRow));
            GeometryFactory factory = new GeometryFactory();
            var polygon = factory.CreatePolygon(coordinates.ToArray());
            polygon.SRID = _spatialReferenceWkid;
            var attributes = new AttributesTable();
            attributes.Add("Id", result.taskId);
            attributes.Add("Description", result.description + "");
            var feature = new Feature(polygon, attributes);
            return feature;
        }

        private Coordinate ConvertPixelToCoordinate(int x ,int y,int tileCol,int tileRow)
        {
            var pntx =  (tileCol * 256 + x) * _mapLods[DownloadLevel].resolution + _originX ;
            var pnty = _originY - ((tileRow+1) * 256 - y) * _mapLods[DownloadLevel].resolution;
            return new Coordinate(pntx, pnty);
        }

        private async Task<IList<IdentityRect>> ProcessJob(DownloadTask task, IDetectEngine detectEngine)
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(8);
            if (task.Tiles.Count < 5) return null;

            await Task.WhenAll(task.Tiles.Select(async tile => {
                await semaphore.WaitAsync();
                try
                {
                    var client = _clientFactory.CreateClient();
                    CancellationTokenSource cts = new CancellationTokenSource(6000);
                    var request = new HttpRequestMessage(HttpMethod.Get, _targetObject.mapServiceUrl + $"/tile/{tile.level}/{tile.row}/{tile.col}");
                    request.Headers.Add("User-Agent", "HttpClientFactory-AI");
                    var response = await client.SendAsync(request,cts.Token);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        tile.data = await response.Content.ReadAsByteArrayAsync();
                    }
                    else
                    {
                        _logger.LogError($"Get Remote Data From /tile/{tile.level}/{tile.row}/{tile.col} failed! ");
                    }
                    
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                    {
                        _logger.LogError($"Get Remote Data From /tile/{tile.level}/{tile.row}/{tile.col} failed! {ex.Message}");
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }));

            //下载完成后开始拼图

            task.TaskData = MergeTile(task.Tiles, task.TaskId);
            using(var ms = new MemoryStream(task.TaskData))
            {
                var ret = detectEngine.ProcessDetectorResult(ms);
                if(ret != null)
                {
                    foreach(var item in ret)
                    {
                        item.taskId = task.TaskId;
                    }
                    return ret;
                }
            }

            return null;

        }


        private byte[] MergeTile(List<TileInfo> list,string taskid)
        {
            if(list.Count == 0 ) return null;
            //如果瓦片数量少于拼图瓦片要求的一半，则不拼图
            if (list.Count(p => p.data != null) < DrawImageCol * DrawImageRow / 2) return null;
            int RowCount = list.GroupBy(p=>p.row).Count();
            int ColCount = list.GroupBy(p=>p.col).Count();
            Bitmap result = new Bitmap(ColCount * 256, RowCount * 256);
            Graphics g1 = Graphics.FromImage(result);
            
            var startRow = list.OrderBy(p => p.row).First().row;
            var startCol = list.OrderBy(p => p.col).First().col;
            for(int i = 0; i < RowCount; i++)
            {
                for(int j = 0; j < ColCount; j++)
                {
                    var target = list.Where(p => p.row == startRow + i && p.col == startCol + j).First();
                    Bitmap bmp;
                    if(target.data != null)
                    {
                        using (var ms = new MemoryStream(target.data))
                        {
                            bmp = new Bitmap(ms);
                        }
                    }
                    else
                    {
                        bmp = new Bitmap(256, 256);
                    }
                    g1.DrawImage((Image)bmp, 0 + (j * 256), 0 + (i * 256));
                    bmp.Dispose(); 
                    
                }
            }
            g1.Dispose();
            var filePath = $"{taskid}.jpg";
            result.Save(filePath,ImageFormat.Jpeg);
            using (MemoryStream ms = new MemoryStream())
            {
                result.Save(ms, ImageFormat.Jpeg);
                return ms.GetBuffer();
            }
        }


        

        /// <summary>
        /// 根据空间坐标计算DownloadLod 这个比例尺下的瓦片索引
        /// </summary>
        /// <param name="x">经度</param>
        /// <param name="y">纬度</param>
        /// <returns></returns>
        private TileInfo CalcularTileIndex(float x, float y ,int level = 8)
        {
            var resolution = _mapLods[level].resolution;
            var row = (int)((_originY - y) / (256 * resolution));
            var col = (int)((x - _originX) / (256 * resolution));
            return new TileInfo() { col= col, row = row };
        }

    }
}
