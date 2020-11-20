namespace YoYoWebApp.Core.Enums
{
    public static class AppEnum
    {
        public enum State
        {
            IDLE = 0,
            RUNNING = 1,
            WARNED = 2,
            CANCELLED = 3,
            FINISHED = 4,
            STOPPED = 5
        }

        public enum Trigger
        {
            START = 0,
            WARN = 1,
            CANCEL = 2,
            FINISH = 3,
            STOP = 4
        }
    }
}
