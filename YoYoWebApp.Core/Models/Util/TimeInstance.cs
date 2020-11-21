namespace YoYoWebApp.Core.Models.Util
{
    public class TimeInstance
    {
        public TimeInstance(int minute, int second, int milisecond)
        {
            this.Minute = minute;
            this.Second = second;
            this.MilliSecond = milisecond;
        }
        public int Minute { get; set; }

        public int Second { get; set; }

        public long MilliSecond { get; set; }

        public int TotalSeconds
        {
            get
            {
                return (Minute * 60) + Second;
            }
        }
    }
}
