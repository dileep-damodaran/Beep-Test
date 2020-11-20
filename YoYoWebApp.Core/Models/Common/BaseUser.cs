using YoYoWebApp.Core.Interfaces.User;
using static YoYoWebApp.Core.Enums.AppEnum;

namespace YoYoWebApp.Core.Models.Common
{
    public abstract class BaseUser : IBaseUser
    {
        public BaseUser()
        {

        }

        public int Id { get; set; }

        public string Name { get; set; }
    }
}
