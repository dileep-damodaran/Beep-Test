using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoYoWebApp.Core.Enums;
using YoYoWebApp.Core.Models.Athletes;
using YoYoWebApp.Core.Models.Schema;
using YoYoWebApp.Core.Models.Test;
using YoYoWebApp.Core.Models.Util;

namespace YoYoWebApp.Infra.Manager
{
    public sealed class TestSession
    {

        private static TestSession _instance;

        private TestSession()
        {
            CancellationTokenSource = new CancellationTokenSource();
            Athletes = new List<Athlete>();
        }

        public static TestSession Instance
        {
            get
            {

                if (_instance == null)
                    _instance = new TestSession();

                return _instance;
            }
        }

        private WebSocket WebSocket { get; set; }

        private CancellationTokenSource CancellationTokenSource { get; set; }


        private System.Timers.Timer Timer;

        public List<Athlete> Athletes { get; set; }

        private TimeInstance CurrentTime { get; set; }

        private List<SchemaInstance> Schema { get; set; }

        public SchemaInstance Previous { get; private set; }

        public SchemaInstance Current { get; private set; }

        public int CurrentIndex
        {
            get
            {
                return Current != null ? Schema.IndexOf(Current) : -1;
            }
        }

        public SchemaInstance Next
        {
            get
            {
                int nextIndex = CurrentIndex == -1 ? -1 : CurrentIndex < (Schema.Count() - 1) ? CurrentIndex + 1 : -1;

                return nextIndex != -1 ? Schema[nextIndex] : null;
            }
        }

        private int? NextIntervalInSecond
        {
            get
            {
                return (Next?.StartTime?.Minute * 60 * 1000) + (Next?.StartTime?.Second * 1000);
            }
        }

        public SchemaInstance Last { get; private set; }

        public void Initialize(WebSocket socket, IEnumerable<SchemaInstance> schema)
        {
            if (schema == null || !schema.Any())
                throw new ArgumentNullException(nameof(schema));


            WebSocket = socket;
            Schema = schema.OrderBy(_x => _x.StartTime.TotalSeconds).ToList();

            Previous = null;
            Current = Schema.FirstOrDefault();
            Last = Schema.LastOrDefault();

            Start();
        }

        private async Task Start()
        {
            CancellationToken cancellationToken = CancellationTokenSource.Token;
            ToggleTimer();
            await Task.Factory.StartNew(() => Process(), cancellationToken);
        }

        public void Stop()
        {
            var allowedAthletes = Athletes.Where(_x => _x.State == AppEnum.State.RUNNING || _x.State == AppEnum.State.WARNED).ToList();

            allowedAthletes.ForEach(_x =>
            {
                _x.Finish();
            });

            OnStopEvent();
            CancellationTokenSource.Cancel();
            ToggleTimer(false);
        }

        private void ToggleTimer(bool start = true)
        {
            if (start)
            {
                this.Timer = new System.Timers.Timer(1);
                this.Timer.Elapsed += OnTimedEvent;
                this.Timer.AutoReset = true;
                this.Timer.Enabled = true;

                this.Timer.Start();
            }
            else
                this.Timer.Stop();
        }

        public void AddAthletes(List<Athlete> athletes)
        {
            if (athletes.Any())
                athletes.ForEach(_x => { _x.Start(); Athletes.Add(_x); });
        }

        public void WarnAthelete(int id)
        {
            if (id == default)
                throw new ArgumentNullException(nameof(id));

            foreach (var athlete in Athletes)
            {
                bool found = athlete.Id == id;

                if (found && athlete.CanWarn)
                {
                    athlete.Warn();
                    break;
                }
            }
        }

        public void StopAthelete(int id)
        {
            if (id == default)
                throw new ArgumentNullException(nameof(id));

            int remainingAthleteToComplete = 0;

            foreach (var athlete in Athletes)
            {
                bool found = athlete.Id == id;

                if (found && athlete.CanStop)
                {
                    athlete.Stop();
                    athlete.Result = new TestResult(Previous?.SpeedLevel, Previous?.ShuttleNo);
                }

                if (!athlete.State.Equals(AppEnum.State.STOPPED))
                    remainingAthleteToComplete++;
            }

            if (remainingAthleteToComplete == 0)
                Stop();
        }


        private async void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            var dashboardModel = new TestMetaData
            {
                IsLastShuttle = Current == Last,
                IsCompleted = Current == null && Next == null,
                NextShuttle = Next != null ? Next.StartTime : Current.StartTime,
                TotalTime = CurrentTime,
                TotalDistance = Current.AccumulatedShuttleDistance,
                Level = Current.SpeedLevel,
                Shuttle = Current.ShuttleNo,
                Speed = Current.Speed,
                Athletes = Athletes
            };

            float shuttleDurationInSeconds = Next != null ? Next.StartTime.TotalSeconds - Current.StartTime.TotalSeconds : 1;
            int secondsPassed = CurrentTime.TotalSeconds - Current.StartTime.TotalSeconds;
            dashboardModel.Progress = (int)((secondsPassed / shuttleDurationInSeconds) * 100);

            byte[] bytes;
            var obj = dashboardModel;
            var data = JsonConvert.SerializeObject(obj, Formatting.Indented);

            bytes = Encoding.ASCII.GetBytes(data);
            var arraySegment = new ArraySegment<byte>(bytes);

            await this.WebSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async void OnStopEvent()
        {
            var meta = new TestMetaData
            {
                IsLastShuttle = Current == Last,
                IsCompleted = Current == null && Next == null,
                NextShuttle = Next != null ? Next.StartTime : Current.StartTime,
                TotalTime = CurrentTime,
                TotalDistance = Current.AccumulatedShuttleDistance,
                Level = Current.SpeedLevel,
                Shuttle = Current.ShuttleNo,
                Speed = Current.Speed,
                Athletes = Athletes,
                Progress = 0
            };

            byte[] bytes;
            var obj = meta;
            var data = JsonConvert.SerializeObject(obj, Formatting.Indented);

            bytes = Encoding.ASCII.GetBytes(data);
            var arraySegment = new ArraySegment<byte>(bytes);

            await this.WebSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private void Process()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.IsRunning)
            {
                int elapsedSeconds = (int)(stopwatch.ElapsedMilliseconds / 1000);

                if (elapsedSeconds >= (NextIntervalInSecond / 1000))
                {
                    Previous = Current;
                    Current = Next;

                    stopwatch.Stop();
                    stopwatch.Start();
                }

                int minute = (elapsedSeconds / 60);
                int seconds = (int)(elapsedSeconds % 60);

                var tObj = new TimeInstance(minute, seconds, 0);


                tObj.MilliSecond = stopwatch.ElapsedMilliseconds - (((minute * 60) + seconds) * 1000);
                this.CurrentTime = tObj;
            }
        }
    }
}
