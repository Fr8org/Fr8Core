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
using Hub.Managers;

namespace Web.Controllers
{
    [RoutePrefix("manifests")]
    public class ManifestController : ApiController
    {
        private IManifest _manifest;
        private ICrateManager _crateManager;

        public ManifestController()
        {
            _manifest = ObjectFactory.GetInstance<IManifest>();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var crate = _manifest.GetById(id);
            if (crate != null)
            {
                return Ok(_crateManager.SerializeToProxy(crate));
            }
            return Ok();
        }
    }
}
