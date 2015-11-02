using Hub;
using Hub.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;

namespace HubWeb.Controllers
{
    [RoutePrefix("manifests")]
    public class ManifestController : ApiController
    {
        private IManifest _manifest;

        public ManifestController()
        {
            _manifest = ObjectFactory.GetInstance<IManifest>();

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
