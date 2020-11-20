using System;
using System.IO;
using YoYoWebApp.Core.Interfaces.Configuration;
using static YoYoWebApp.Core.Constants.Constant;

namespace YoYoWebApp.Infra.Configuration
{
    public class AppConfiguration : IAppConfiguration
    {

        private readonly string _root;

        public AppConfiguration(string contentRootPath)
        {
            if (string.IsNullOrWhiteSpace(contentRootPath))
                throw new ArgumentNullException(nameof(contentRootPath));

            _root = contentRootPath;
        }

        public string SchemaPath => Path.Combine(_root, "..\\", AppEssentialPath.SchemaPath);

        public string AthleteSchemaPath => Path.Combine(_root, "..\\", AppEssentialPath.AthleteSchemaPath);
    }
}
