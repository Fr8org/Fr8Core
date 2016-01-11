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
        IList<TagDO> GetAll();

        TagDO GetById(int id);

        TagDO GetByKey(string key);

        IList<TagDO> GetList(int fileDoId);

        IList<FileDO> GetFiles(TagDO tag);

        IList<FileDO> GetFiles(int tagId);

        void UpdateByKey(string key, string value);

        void UpdateAll(int fileDoId, IList<TagDO> tags);

        void RemoveAll(int fileDoId);

        void Remove(int fileDoId, int tagId);

        void Add(int fileDoId, TagDO tag);
    }
}
