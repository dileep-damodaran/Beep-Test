using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoYoWebApp.Core.Interfaces.Repositories.Schema;
using YoYoWebApp.Core.Models.Athletes;
using YoYoWebApp.Core.Models.Schema;
using YoYoWebApp.Core.Models.Util.SchemaReader;
using YoYoWebApp.Infra.Manager;

namespace YoYoWebApp.Controllers.Test
{

    public class TestController : Controller
    {

        private readonly IMapper _mapper;

        private readonly ISchemaRepository _schema;
        public TestController(IMapper mapper, ISchemaRepository schema)
        {
            _mapper = mapper;
            _schema = schema;
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task Start()
        {
            var context = ControllerContext.HttpContext;

            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            if (!isSocketRequest)
                context.Response.StatusCode = 400;

            var socket = await context.WebSockets.AcceptWebSocketAsync();

            var getSchemaTask = _schema.GetSchema();

            if (!getSchemaTask.IsSucceeded)
                throw new System.Exception("No Schema found.");

            var getAthletesTask = _schema.GetAthletes();

            if (!getAthletesTask.IsSucceeded)
                throw new System.Exception("No athletes found.");

            var schema = _mapper.Map<IEnumerable<SchemaInstance>>((IEnumerable<SchemaInstanceReaderModel>)getSchemaTask.Result);

            var athletes = (List<Athlete>)getAthletesTask.Result;

            var session = TestSession.Instance;

            session.AddAthletes(athletes);

            session.Initialize(socket, schema);

            while (!session.IsCompleted)
            {

            }
        }

        [HttpPost]
        public IActionResult Stop()
        {
            var session = TestSession.Instance;

            session.Stop();

            return Ok();
        }

        [HttpGet]
        public IActionResult GetAthletes()
        {
            var athletes = (IEnumerable<Athlete>)_schema.GetAthletes().Result;

            return Ok(athletes);
        }


        [HttpPost]
        public IActionResult WarnAthlete(int id)
        {
            if (id == default)
                return BadRequest();

            TestSession.Instance.WarnAthelete(id);

            return Ok();
        }


        [HttpPost]
        public IActionResult StopAthlete(int id)
        {
            if (id == default)
                return BadRequest();

            TestSession.Instance.StopAthelete(id);

            return Ok();
        }
    }
}
