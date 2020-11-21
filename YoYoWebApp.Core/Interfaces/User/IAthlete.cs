using YoYoWebApp.Core.Models.Test;
using static YoYoWebApp.Core.Enums.AppEnum;

namespace YoYoWebApp.Core.Interfaces.User
{
    public interface IAthlete : IBaseUser
    {
        State State { get; set; }

        TestResult Result { get; set; }
    }
}
