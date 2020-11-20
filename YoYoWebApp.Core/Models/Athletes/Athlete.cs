using Stateless;
using System;
using System.Collections.Generic;
using YoYoWebApp.Core.Enums;
using YoYoWebApp.Core.Interfaces.User;
using YoYoWebApp.Core.Models.Test;

namespace YoYoWebApp.Core.Models.Athletes
{
    public class Athlete : IAthlete
    {

        public Athlete()
        {
            ConfigureStateMachine();
            State = AppEnum.State.IDLE;
        }


        public StateMachine<AppEnum.State, AppEnum.Trigger> StateMachine { get; set; }

        public int Id { get; set; }


        public string Name { get; set; }


        public AppEnum.State State { get; set; }


        public TestResult Result { get; set; }


        public string StateString
        {
            get
            {
                return Enum.GetName(typeof(AppEnum.State), State);
            }
        }

        public bool CanWarn
        {

            get
            {
                return State.Equals(AppEnum.State.RUNNING);
            }
        }

        public bool CanStop
        {

            get
            {
                var allowedStates = new List<AppEnum.State>() { AppEnum.State.RUNNING, AppEnum.State.WARNED };

                return allowedStates.Contains(State);
            }
        }


        public void Start()
        {
            StateMachine.Fire(AppEnum.Trigger.START);
        }

        public void Warn()
        {

            StateMachine.Fire(AppEnum.Trigger.WARN);
        }

        public void Stop()
        {
            StateMachine.Fire(AppEnum.Trigger.STOP);
        }


        public void Finish()
        {
            StateMachine.Fire(AppEnum.Trigger.FINISH);
        }

        public void ConfigureStateMachine()
        {
            this.StateMachine = new StateMachine<AppEnum.State, AppEnum.Trigger>(() => State, s => State = s);

            this.StateMachine.Configure(AppEnum.State.IDLE)
                .Permit(AppEnum.Trigger.START, AppEnum.State.RUNNING);

            this.StateMachine.Configure(AppEnum.State.RUNNING)
                .Permit(AppEnum.Trigger.WARN, AppEnum.State.WARNED)
                .Permit(AppEnum.Trigger.STOP, AppEnum.State.STOPPED)
                .Permit(AppEnum.Trigger.FINISH, AppEnum.State.FINISHED);

            this.StateMachine.Configure(AppEnum.State.WARNED)
                .Permit(AppEnum.Trigger.STOP, AppEnum.State.STOPPED)
                .Permit(AppEnum.Trigger.FINISH, AppEnum.State.FINISHED);

        }
    }
}
