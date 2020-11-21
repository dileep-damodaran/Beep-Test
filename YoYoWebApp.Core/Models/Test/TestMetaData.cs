using System.Collections.Generic;
using YoYoWebApp.Core.Models.Athletes;
using YoYoWebApp.Core.Models.Util;

namespace YoYoWebApp.Core.Models.Test
{
    public class TestMetaData
    {

        public TestMetaData()
        {

        }

        public TimeInstance NextShuttle { get; set; }

        public TimeInstance TotalTime { get; set; }

        public float TotalDistance { get; set; }

        public int Level { get; set; }

        public int Shuttle { get; set; }

        public float Speed { get; set; }

        public int Progress { get; set; }

        public bool IsLastShuttle { get; set; }

        public bool IsCompleted { get; set; }

        public List<Athlete> Athletes { get; set; }
    }
}
