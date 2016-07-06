using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using terminalAsana.Asana.Entities;

namespace terminalAsana.Interfaces
{
    public interface IAsanaTasks
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        Task<AsanaTask>                 CreateAsync(AsanaTask task);

        /// <summary>
        /// Returns the complete task record for a single task
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<AsanaTask>                 GetAsync(string id);

        /// <summary>
        /// Only the fields provided in the data block will be updated; any unspecified fields will remain unchanged.
        /// </summary>
        /// <param name="task"></param>
        /// <returns>Returns the complete updated task record.</returns>
        Task<AsanaTask>                 UpdateAsync(AsanaTask task);

        /// <summary>
        ///  Deleted tasks go into the “trash” of the user making the delete request. Tasks can be recovered from the trash within a period of 30 days; afterward they are completely removed from the system.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        Task                            DeleteAsync(AsanaTask task);

        /// <summary>
        /// Returns the compact task records for some filtered set of tasks.
        /// </summary>
        /// <param name="query">You must specify a project or tag if you do not specify assignee and workspace</param>
        /// <returns></returns>
        Task<IEnumerable<AsanaTask>>    GetAsync(AsanaTaskQuery query);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        Task<IEnumerable<AsanaTask>>    GetAllSubtasksAsync(string taskId);

        /// <summary>
        /// Creates a new subtask and adds it to the parent task. 
        /// </summary>
        /// <param name="task"></param>
        /// <returns>Returns the full record for the newly created subtask.</returns>
        Task<AsanaTask>                 CreateSubTaskAsync(AsanaTask task);


        /// <summary>
        /// Changes the parent of a task. Each task may only be a subtask of a single parent, or no parent task at all.. 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        Task                            SetParentTaskAsync(AsanaTask task);

        /// <summary>
        /// Returns a compact representation of all of the stories on the task
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        Task<IEnumerable<AsanaStory>>   GetStoriesAsync(string taskId);

        /// <summary>
        /// Returns a compact representation of all of the projects the task is in
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        Task<IEnumerable<AsanaProject>> GetProjectsAsync(string taskId);


        /// <summary>
        /// Adds the task to the specified project, in the optional location specified. If no location arguments are given, the task will be added to the beginning of the project.
        /// </summary>
        /// <returns>Returns an empty data block.</returns>
        Task                            AddToProjectAsync(AsanaProjectInsertion query);

        /// <summary>
        /// Removes the task from the specified project. The task will still exist in the system, but it will not be in the project anymore.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task                            RemoveFromProject(string taskId, string projectId);

        /// <summary>
        /// Returns a compact representation of all of the tags the task has
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        Task<IEnumerable<AsanaTag>>     GetTags(string taskId);

        /// <summary>
        /// Adds a tag to a task. Returns an empty data block.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        Task                            AddTag(string taskId, string tagId);

        /// <summary>
        /// Removes a tag from the task. Returns an empty data block.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        Task                            RemoveTag(string taskId, string tagId);

        /// <summary>
        /// Adds each of the specified followers to the task, if they are not already following. Returns the complete, updated record for the affected task
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="followers">array of folowers Id`s</param>
        /// <returns></returns>
        Task<AsanaTask>                 AddFollowers(string taskId, IEnumerable<AsanaUser> followers);

        /// <summary>
        /// Removes each of the specified followers from the task if they are following. Returns the complete, updated record for the affected task.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="followers">array of folowers Id`s</param>
        /// <returns></returns>
        Task<AsanaTask>                 RemoveFollowers(string taskId, IEnumerable<AsanaUser> followers);
    }
}
