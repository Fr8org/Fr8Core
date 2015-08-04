using System;
using System.Web.Http;
using AutoMapper;
using Data.Entities;

namespace Web.Controllers.Helpers
{
    public static class MappingHelpers
    {
        public static TDestination ConvertTo<TSource, TDestination>(this ApiController controller
            , TSource source
            , Action<TSource, TDestination> additionalConverstion = null)
        {
            var destination = Mapper.Map<TSource, TDestination>(source);
            if (additionalConverstion != null)
                additionalConverstion(source, destination);
            return destination;
        }

    }
}