using Core;
using Core.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;

namespace Web.Controllers
{
    [RoutePrefix("manifest")]
    public class ManifestController : ApiController
    {
        private Core.Interfaces.IManifest _manifest;

        public ManifestController()
        {
            _manifest = ObjectFactory.GetInstance<Core.Interfaces.IManifest>();

        }

        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var crate = _manifest.GetById(id);
            if (crate != null)
            {
                return Ok(crate);
            }
            return Ok();
        }
    }
}
