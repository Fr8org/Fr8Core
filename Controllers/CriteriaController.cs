using System;
using System.Web.Http;
using System.Web.Http.Description;
using Web.ViewModels;

namespace Web.Controllers
{
    /// <summary>
    /// Critera web api controller to handle CRUD operations from frontend.
    /// </summary>
    [RoutePrefix("api/criteria")]
    public class CriteriaController : ApiController
    {
        /// <summary>
        /// Retrieve criteria by id.
        /// </summary>
        /// <param name="id">Criteria id.</param>
        [ResponseType(typeof(CriteriaDTO))]
        public IHttpActionResult Get(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Recieve criteria with temporary id, create criteria,
        /// and return criteria with global id.
        /// </summary>
        /// <param name="dto">Criteria data transfer object.</param>
        /// <returns>Created criteria with global id.</returns>
        [ResponseType(typeof(CriteriaDTO))]
        public IHttpActionResult Post(CriteriaDTO dto)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Recieve criteria with global id, update criteria,
        /// and return updated criteria.
        /// </summary>
        /// <param name="dto">Criteria data transfer object.</param>
        /// <returns>Updated criteria.</returns>
        [ResponseType(typeof(CriteriaDTO))]
        public IHttpActionResult Put(CriteriaDTO dto)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete criteria by id provided.
        /// </summary>
        /// <param name="id">Criteria id.</param>
        /// <returns>Deleted criteria.</returns>
        [ResponseType(typeof(void))]
        public IHttpActionResult Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}