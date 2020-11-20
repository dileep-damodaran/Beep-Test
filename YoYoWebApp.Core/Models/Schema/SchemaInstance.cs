using YoYoWebApp.Core.Models.Util;

namespace YoYoWebApp.Core.Models.Schema
{
    public class SchemaInstance
    {

        public SchemaInstance()
        {

        }

        public int AccumulatedShuttleDistance { get; set; }

        public int SpeedLevel { get; set; }

        public int ShuttleNo { get; set; }

        public float Speed { get; set; }

        public float LevelTime { get; set; }

        public TimeInstance CommulativeTime { get; set; }

        public TimeInstance StartTime { get; set; }
    }
}
