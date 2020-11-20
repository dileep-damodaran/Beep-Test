using System;

namespace YoYoWebApp.Core.Models.Test
{
    public class TestResult
    {

        public TestResult(int? level, int? shuttle)
        {

            SpeedLevel = level ?? 0;
            ShuttleNo = shuttle ?? 0;
        }

        public int SpeedLevel { get; set; }

        public int ShuttleNo { get; set; }
    }
}
