using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YoYoWebApp.Core.Interfaces.Configuration;
using YoYoWebApp.Core.Interfaces.Repositories.Schema;
using YoYoWebApp.Core.Models.Athletes;
using YoYoWebApp.Core.Models.Schema;
using YoYoWebApp.Core.Models.Util;
using YoYoWebApp.Core.Models.Util.SchemaReader;

namespace YoYoWebApp.Infra.Repository.Schema
{
    public class SchemaRepository : ISchemaRepository
    {

        private readonly IAppConfiguration _configuration;


        public SchemaRepository(IAppConfiguration configuration)
        {

            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public AppAction GetAthletes()
        {
            bool hasError = false;
            AppAction result = new AppAction(!hasError, null);

            string schemaPath = this._configuration.AthleteSchemaPath;

            if (!File.Exists(schemaPath))
                return result;

            string content = File.ReadAllText(schemaPath);

            IEnumerable<Athlete> schema = JsonConvert.DeserializeObject<IEnumerable<Athlete>>(content);

            result.Result = schema;

            return result;
        }

        public AppAction GetSchema()
        {
            bool hasError = false;
            AppAction result = new AppAction(!hasError, null);

            string schemaPath = this._configuration.SchemaPath;

            if (!File.Exists(schemaPath))
                return result;

            string content = File.ReadAllText(schemaPath);

            IEnumerable<SchemaInstanceReaderModel> schema = JsonConvert.DeserializeObject<IEnumerable<SchemaInstanceReaderModel>>(content);

            result.Result = schema;

            return result;
        }
    }
}
