using AutoMapper;
using System;
using System.Linq;
using YoYoWebApp.Core.Models.Schema;
using YoYoWebApp.Core.Models.Util;
using YoYoWebApp.Core.Models.Util.SchemaReader;

namespace YoYoWebApp.Configuration
{
    public class AutoMapperConfigurationManager : Profile
    {
        public AutoMapperConfigurationManager()
        {
            CreateMap<SchemaInstanceReaderModel, SchemaInstance>()
                .ForMember(dest =>
            dest.CommulativeTime,
            opt => opt.MapFrom(src =>
            new TimeInstance(Int32.Parse(src.CommulativeTime.Split(new char[] { ':' }).FirstOrDefault()),
            Int32.Parse(src.CommulativeTime.Split(new char[] { ':' }).LastOrDefault()), 0)
            ))
                .ForMember(dest =>
            dest.StartTime,
            opt => opt.MapFrom(src =>
            new TimeInstance(Int32.Parse(src.StartTime.Split(new char[] { ':' }).FirstOrDefault()),
            Int32.Parse(src.StartTime.Split(new char[] { ':' }).LastOrDefault()), 0)
            ));
        }

    }
}

