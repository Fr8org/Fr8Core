using Data.Entities;
using Data.Interfaces;
using Data.Utility;
using Data.Utility.JoinClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Interfaces
{
    /// <summary>
    /// Interface for Tags service
    /// </summary>
    public interface ITag
    {
        IList<Tag> GetAllTags();

        Tag GetTag(int id);

        Tag GetTagByKey(string key);

        IList<Tag> GetTags(int fileDoId);

        IList<FileDO> GetFiles(Tag tag);

        IList<FileDO> GetFiles(int tagId);

        void UpdateTagByKey(string key, string value);

        void UpdateAllFileTags(int fileDoId, IList<Tag> tags);

        void RemoveAllFileTags(int fileDoId);

        void RemoveFileTag(int fileDoId, int tagId);

        void AddFileTag(int fileDoId, Tag tag);
    }
}
