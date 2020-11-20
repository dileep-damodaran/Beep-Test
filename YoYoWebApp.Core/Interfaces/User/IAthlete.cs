using Stateless;
using static YoYoWebApp.Core.Enums.AppEnum;

namespace YoYoWebApp.Core.Interfaces.User
{
    public interface IAthlete : IBaseUser
    {

        State State { get; set; }

        StateMachine<State, Trigger> StateMachine { get; set; }

        void ConfigureStateMachine();
    }
}
