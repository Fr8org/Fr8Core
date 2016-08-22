using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using terminalAsana.Asana.Entities;

namespace terminalAsana.Interfaces
{
    public interface IAsanaStories
    {
        /// <summary>
        /// Returns the compact records for all stories on the task.
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        Task<IEnumerable<AsanaStory>> GetTaskStoriesAsync(string taskId);

        /// <summary>
        /// Returns the full record for a single story.
        /// </summary>
        /// <param name="storyId"></param>
        /// <returns></returns>
        Task<AsanaStory> GetAsync(string storyId);

        /// <summary>
        /// Adds a comment to a task. The comment will be authored by the currently authenticated user, and timestamped when the server receives the request.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="commentText"></param>
        /// <returns>Returns the full record for the new story added to the task.</returns>
        Task<AsanaStory> PostCommentAsync(string taskId, string commentText);

    }
}
