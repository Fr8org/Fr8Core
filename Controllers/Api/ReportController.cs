using System;
using System.Net;
using System.Web.Http;
using StructureMap;
using Hub.Infrastructure;
using Hub.Interfaces;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.States;
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class ReportController : ApiController
    {
        private readonly IReport _report;
        private readonly ISecurityServices _securityService;
        private readonly IUnitOfWorkFactory _uowFactory;

        public ReportController(IReport report, ISecurityServices securityService, IUnitOfWorkFactory uowFactory)
        {
            _report = report;
            _securityService = securityService;
            _uowFactory = uowFactory;
        }
        /// <summary>
        /// Retrieves collection of log records based on query parameters specified
        /// </summary>
        /// <param name="type">Type of log records to return. Supports values of 'incidents' and 'facts'</param>
        /// <param name="page">Ordinal number of subset of log records to retrieve</param>
        /// <param name="isDescending">Whether to perform sort of results in descending order</param>
        /// <param name="isCurrentUser">Whether to show log records of current user only</param>
        /// <param name="itemPerPage">Max number of log records to retrieve in single response</param>
        /// <param name="filter">Part of textual field of log record to filter by</param>
        /// <remarks>
        /// Fr8 authentication headers must be provided
        /// </remarks>
        [Fr8ApiAuthorize]
        [HttpGet]
        [ResponseType(typeof(HistoryResultDTO<FactDTO>))]
        [ResponseType(typeof(HistoryResultDTO<IncidentDTO>))]
        [SwaggerResponse(HttpStatusCode.OK, "Collection of log records", typeof(HistoryResultDTO<IncidentDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Incorrect type is specified")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request")]
        public IHttpActionResult Get([FromUri] string type, [FromUri] HistoryQueryDTO historyQueryDTO)
        {
            //Only admins can see logs of other users
            if (!historyQueryDTO.IsCurrentUser && !HasManageUserPrivilege())
            {
                return new StatusCodeResult(HttpStatusCode.Forbidden, Request);
            }
            type = (type ?? string.Empty).Trim().ToLower();
            using (var uow = _uowFactory.Create())
            {
                if (type == "incidents")
                {
                    var result = _report.GetIncidents(uow, historyQueryDTO);
                    return Ok(result);
                }
                else if (type == "facts")
                {
                    var result = _report.GetFacts(uow, historyQueryDTO);
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Specified report type is not supported");
                }
            }
        }
        /// <summary>
        /// Returns a boolean values indicating whether current user can see other user's history items (facts and incidents)
        /// </summary>
        [SwaggerResponse(HttpStatusCode.OK, "Whether user can see other user's history items")]
        [SwaggerResponseRemoveDefaults]
        [AllowAnonymous]
        [HttpGet]
        [ActionName("can_see_other_user_history")]
        public IHttpActionResult CanSeeOtherUserHistory()
        {
            var hasManageUserPrivilege = HasManageUserPrivilege();
            return Ok(new { hasManageUserPrivilege });
        }

        private bool HasManageUserPrivilege()
        {
            return _securityService.IsAuthenticated() && _securityService.UserHasPermission(PermissionType.ManageFr8Users, nameof(Fr8AccountDO));
        }
    }
}