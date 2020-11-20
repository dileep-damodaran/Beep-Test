using static YoYoWebApp.Core.Enums.AppEnum;

namespace YoYoWebApp.Core.Interfaces.User
{
    public interface IBaseUser
    {
        int Id { get; set; }

        string Name { get; set; }
    }
}
