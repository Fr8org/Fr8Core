using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;
using StructureMap;
using Hub.Interfaces;
using HubWeb.Infrastructure_HubWeb;

namespace HubWeb.Controllers
{
    //[RoutePrefix("field")]
    public class FieldController : ApiController
    {
        private readonly IField _field;

        public FieldController()
        {
            _field = ObjectFactory.GetInstance<IField>();
        }

        public FieldController(IField service)
        {
            _field = service;
        }


        [HttpPost]
        //[Fr8ApiAuthorize]
        [ResponseType(typeof(List<FieldValidationResult>))]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Exists(List<FieldValidationDTO> fieldCheckList)
        {
            var result = new List<FieldValidationResult>();
            foreach (var fieldCheck in fieldCheckList)
            {
                result.Add(_field.Exists(fieldCheck) ? FieldValidationResult.Exists : FieldValidationResult.NotExists);
            }
            return Ok(result);
        }    
    }
}