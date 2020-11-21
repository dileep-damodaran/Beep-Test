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
using YoYoWebApp.Core.Interfaces.Test;
using YoYoWebApp.Core.Models.Athletes;
using YoYoWebApp.Core.Models.Schema;
using YoYoWebApp.Core.Models.Test;
using YoYoWebApp.Core.Models.Util;

namespace YoYoWebApp.Infra.Manager
{
    public class TestSession : ITestSession
    {

        private static TestSession _instance;

        private List<Athlete> _athletes;

        private TimeInstance _currentTime;

        private List<SchemaInstance> _schema;

        private SchemaInstance _previous;

        private SchemaInstance _current;

        private SchemaInstance _last;

        private WebSocket _socket;

        private System.Timers.Timer _timer;

        private CancellationTokenSource _cancellationTokenSource;

        private TestSession()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _athletes = new List<Athlete>();
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

        public List<Athlete> Athletes => _athletes;

        public TimeInstance CurrentTime => _currentTime;

        private SchemaInstance Last => _last;

        public bool IsCompleted => !_timer.Enabled;

        private int CurrentIndex
        {
            get
            {
                return _current != null ? _schema.IndexOf(_current) : -1;
            }
        }

        private SchemaInstance Next
        {
            get
            {
                int nextIndex = CurrentIndex == -1 ? -1 : CurrentIndex < (_schema.Count() - 1) ? CurrentIndex + 1 : -1;

                return nextIndex != -1 ? _schema[nextIndex] : null;
            }
        }
        private int? NextIntervalInSecond
        {
            get
            {
                return (Next?.StartTime?.Minute * 60 * 1000) + (Next?.StartTime?.Second * 1000);
            }
        }

        public void Initialize(WebSocket socket, IEnumerable<SchemaInstance> schema)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            if (schema == null || !schema.Any())
                throw new ArgumentNullException(nameof(schema));

            _socket = socket;
            _schema = schema.OrderBy(_x => _x.StartTime.TotalSeconds).ToList();

            _previous = null;
            _current = _schema.FirstOrDefault();
            _last = _schema.LastOrDefault();

            Start();
        }

        private async Task Start()
        {
            CancellationToken cancellationToken = _cancellationTokenSource.Token;
            ToggleTimer();
            await Task.Factory.StartNew(() => MonitorSchema(), cancellationToken);
        }

        public void Stop()
        {
            var allowedAthletes = _athletes.Where(_x => _x.State == AppEnum.State.RUNNING || _x.State == AppEnum.State.WARNED).ToList();

            allowedAthletes.ForEach(_x =>
            {
                _x.Finish();
            });

            OnStopEvent();
            _cancellationTokenSource.Cancel();
            ToggleTimer(false);
        }

        private void ToggleTimer(bool start = true)
        {
            if (!start)
                _timer.Stop();

            else
            {
                _timer = new System.Timers.Timer(1);
                _timer.Elapsed += OnTimedEvent;
                _timer.AutoReset = true;
                _timer.Enabled = true;

                _timer.Start();
            }
        }

        public void AddAthletes(List<Athlete> athletes)
        {
            if (athletes.Any())
                athletes.ForEach(_x => { _x.Start(); _athletes.Add(_x); });
        }

        public void WarnAthelete(int id)
        {
            if (id == default)
                throw new ArgumentNullException(nameof(id));

            foreach (var athlete in _athletes)
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

            foreach (var athlete in _athletes)
            {
                bool found = athlete.Id == id;

                if (found && athlete.CanStop)
                {
                    athlete.Stop();
                    athlete.Result = new TestResult(_previous?.SpeedLevel, _previous?.ShuttleNo);
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
                IsLastShuttle = _current == Last,
                IsCompleted = _current == null && Next == null,
                NextShuttle = Next != null ? Next.StartTime : _current.StartTime,
                TotalTime = CurrentTime,
                TotalDistance = _current.AccumulatedShuttleDistance,
                Level = _current.SpeedLevel,
                Shuttle = _current.ShuttleNo,
                Speed = _current.Speed,
                Athletes = _athletes
            };

            float shuttleDurationInSeconds = Next != null ? Next.StartTime.TotalSeconds - _current.StartTime.TotalSeconds : 1;
            int secondsPassed = CurrentTime.TotalSeconds - _current.StartTime.TotalSeconds;
            dashboardModel.Progress = (int)((secondsPassed / shuttleDurationInSeconds) * 100);

            byte[] bytes;
            var obj = dashboardModel;
            var data = JsonConvert.SerializeObject(obj, Formatting.Indented);

            bytes = Encoding.ASCII.GetBytes(data);
            var arraySegment = new ArraySegment<byte>(bytes);

            await _socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async void OnStopEvent()
        {
            var meta = new TestMetaData
            {
                IsLastShuttle = _current == Last,
                IsCompleted = _current == null && Next == null,
                NextShuttle = Next != null ? Next.StartTime : _current.StartTime,
                TotalTime = CurrentTime,
                TotalDistance = _current.AccumulatedShuttleDistance,
                Level = _current.SpeedLevel,
                Shuttle = _current.ShuttleNo,
                Speed = _current.Speed,
                Athletes = _athletes,
                Progress = default
            };

            byte[] bytes;
            var obj = meta;
            var data = JsonConvert.SerializeObject(obj, Formatting.Indented);

            bytes = Encoding.ASCII.GetBytes(data);
            var arraySegment = new ArraySegment<byte>(bytes);

            await _socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private void MonitorSchema()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.IsRunning)
            {
                int elapsedSeconds = (int)(stopwatch.ElapsedMilliseconds / 1000);

                if (elapsedSeconds >= (NextIntervalInSecond / 1000))
                {
                    _previous = _current;
                    _current = Next;

                    stopwatch.Stop();
                    stopwatch.Start();
                }

                int minute = (elapsedSeconds / 60);
                int seconds = (int)(elapsedSeconds % 60);

                var tObj = new TimeInstance(minute, seconds, 0);


                tObj.MilliSecond = stopwatch.ElapsedMilliseconds - (((minute * 60) + seconds) * 1000);
                _currentTime = tObj;
            }
        }
    }
}
