using System.Collections.Generic;
using YoYoWebApp.Core.Models.Athletes;

namespace YoYoWebApp.Core.Interfaces.Test
{
    public interface ITestSession
    {
        static ITestSession Instance { get; }

        List<Athlete> Athletes { get; }

        bool IsCompleted { get; }
    }
}
