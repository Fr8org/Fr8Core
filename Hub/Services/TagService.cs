using Data.Entities;
using Data.Interfaces;
using Data.Utility;
using Data.Utility.JoinClasses;
using Hub.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Services
{
    public class TagService : ITag
    {

        public IList<Tag> GetAllTags()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                List<Tag> tags = new List<Tag>();
                //tags = uow.FileRepository.FindList(f => f.DockyardAccountID == dockyardAccountId).ToList();
                return tags;
            }
        }

        public Tag GetTag(int id)
        {
            throw new NotImplementedException();
        }

        public Tag GetTagByKey(string key)
        {
            throw new NotImplementedException();
        }

        public IList<Tag> GetTags(int fileDoId)
        {
            throw new NotImplementedException();
        }

        public IList<FileDO> GetFiles(Tag tag)
        {
            throw new NotImplementedException();
        }

        public IList<FileDO> GetFiles(int tagId)
        {
            throw new NotImplementedException();
        }

        public void Add(IUnitOfWork uow, IList<FileTags> fileTags, IList<Tag> tags)
        {
            throw new NotImplementedException();
        }
    }
}
