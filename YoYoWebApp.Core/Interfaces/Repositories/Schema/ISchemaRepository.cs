using YoYoWebApp.Core.Models.Util;

namespace YoYoWebApp.Core.Interfaces.Repositories.Schema
{
    public interface ISchemaRepository
    {
        AppAction GetSchema();

        AppAction GetAthletes();
    }
}
