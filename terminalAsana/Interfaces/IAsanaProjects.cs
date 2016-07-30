using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using terminalAsana.Asana.Entities;

namespace terminalAsana.Interfaces
{
    public interface IAsanaProjects
    {
        /// <summary>
        /// Creates a new project in a workspace or team.
        /// </summary>
        /// <returns></returns>
        Task<AsanaProject> CreateProject(AsanaProject project);

        /// <summary>
        /// Returns the complete project record for a single project.
        /// </summary>
        /// <returns></returns>
        Task<AsanaProject> Get(string projectId);

        /// <summary>
        /// Returns the compact project records for some filtered set of projects. Use one or more of the parameters provided to filter the projects returned.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<IEnumerable<AsanaProject>> Get(AsanaProjectQuery query);

        /// <summary>
        /// A specific, existing project can be updated by making a PUT request on the URL for that project. Only the fields provided in the data block will be updated; any unspecified fields will remain unchanged.
        /// When using this method, it is best to specify only those fields you wish to change, or else you may overwrite changes made by another user since you last retrieved the task.
        /// </summary>
        /// <returns>Returns the complete updated project record.</returns>
        Task<AsanaProject> Update(AsanaProject project);

        /// <summary>
        /// A specific, existing project can be deleted by making a DELETE request on the URL for that project
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>Returns an empty data record.</returns>
        Task Delete(string projectId);

        /// <summary>
        /// Returns the compact task records for all tasks within the given project, ordered by their priority within the project. Tasks can exist in more than one project at a time.
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<IEnumerable<AsanaTask>> GetTasks(string projectId);

        /// <summary>
        /// Sections are tasks whose names end with a colon character : . For instance sections will be included in query results for tasks and be represented with the same fields. The memberships property of a task contains the project/section pairs a task belongs to when applicable
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<IEnumerable<AsanaSection>> GetSections(string projectId);

    }
}
