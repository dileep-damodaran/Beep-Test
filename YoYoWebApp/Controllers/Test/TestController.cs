﻿using AutoMapper;
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

            var schema = _mapper.Map<IEnumerable<SchemaInstance>>((IEnumerable<SchemaInstanceReaderModel>)_schema.GetSchema().Result);
            var socket = await context.WebSockets.AcceptWebSocketAsync();

            var getAthletesTask = _schema.GetAthletes();

            if (!getAthletesTask.IsSucceeded)
                throw new System.Exception("No athletes found.");

            var athletes = (List<Athlete>)getAthletesTask.Result;

            var session = TestSession.Instance;

            session.AddAthletes(athletes);

            session.Initialize(socket, schema);

            while (true)
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

            foreach (var athlete in TestSession.Instance.Athletes)
            {
                bool found = athlete.Id == id;

                if (found && athlete.CanWarn)
                {
                    athlete.Warn();
                    break;
                }
            }

            return Ok();
        }
    }
}
