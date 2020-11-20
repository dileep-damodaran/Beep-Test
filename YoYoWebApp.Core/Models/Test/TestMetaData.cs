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

        public TimeInstance NextShuttle { get; set; } //_next.StartTime

        public TimeInstance TotalTime { get; set; } //timer

        public float TotalDistance { get; set; } // _current.AccumulatedShuttleDistance

        public int Level { get; set; } //_current.SppedLevel

        public int Shuttle { get; set; } //_current.ShuttleNo

        public float Speed { get; set; } //_current.Speed

        public float Progress { get; set; } //timer

        public bool IsLastShuttle { get; set; } //timer

        public bool IsCompleted { get; set; } 

        public List<Athlete> Athletes { get; set; }
    }
}
