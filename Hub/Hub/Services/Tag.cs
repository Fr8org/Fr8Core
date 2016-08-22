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
    public class Tag : ITag
    {
        public IList<TagDO> GetAll()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var tags = new List<TagDO>();
                tags = uow.TagRepository.GetAll().ToList();
                return tags;
            }
        }

        public TagDO GetById(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.TagRepository.GetByKey(id);
            }
        }

        public TagDO GetByKey(string key)
        { 
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                TagDO tag = null;
                var result = uow.TagRepository.FindOne(x => x.Key == key);
                if (!object.Equals(result, default(TagDO)))
                {
                    tag = result;
                }

                return tag;
            }
        }

        public IList<TagDO> GetList(int fileDoId)
        {
            var tags = new List<TagDO>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fileTags = uow.FileTagsRepository.FindList(x => x.FileDoId == fileDoId);
                foreach (var fileTag in fileTags)
                {
                    tags.Add(fileTag.Tag);
                }
            }
            return tags;
        }

        public IList<FileDO> GetFiles(TagDO tag)
        {
            return GetFiles(tag.Id);
        }

        public IList<FileDO> GetFiles(int tagId)
        {
            var files = new List<FileDO>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var tag = uow.TagRepository.GetByKey(tagId);
                if (tag != null)
                {
                    var fileTags = uow.FileTagsRepository.FindList(x => x.TagId == tag.Id);
                    foreach (var fileTag in fileTags)
                    {
                        var file = uow.FileRepository.FindOne(x => x.Id == fileTag.FileDoId);
                        files.Add(file);
                    }
                }
            }
            return files;
        }

        public void UpdateAll(int fileDoId, IList<TagDO> tags)
        {   
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var tagRepository = uow.TagRepository;
                var fileTagRepository = uow.FileTagsRepository;
                // update old tags and add new
                foreach (var tag in tags)
                {   
                    var dbTag = tagRepository.FindOne(x => x.Key == tag.Key);
                    if (!object.Equals(dbTag, default(TagDO)))
                    {
                        dbTag.Value = tag.Value;
                    }
                    else
                    {
                        tagRepository.Add(tag); 
                    }
                }
                uow.SaveChanges();

                // remove all fileTags with fileDoId
                RemoveAll(fileDoId);

                // Add new fileTags to the fileDo
                foreach (var tag in tags)
                {
                    var dbTag = tagRepository.FindOne(x => x.Key == tag.Key);
                    if (!object.Equals(dbTag, default(TagDO)))
                    {
                        var fileTag = new FileTags()
                        {
                            FileDoId = fileDoId,
                            TagId = dbTag.Id
                        };

                        fileTagRepository.Add(fileTag);
                    }
                }
                uow.SaveChanges();
            }
        }

        public void RemoveAll(int fileDoId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fileTagsRepository = uow.FileTagsRepository;
                var listFileTags = fileTagsRepository.FindList(x => x.FileDoId == fileDoId);
                foreach (var fileTag in listFileTags)
                {
                    fileTagsRepository.Remove(fileTag);
                }

                uow.SaveChanges();
            }
        }

        public void Remove(int fileDoId, int tagId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fileTagsRepository = uow.FileTagsRepository;
                var fileTag = fileTagsRepository.FindOne(x => x.FileDoId == fileDoId && x.TagId == tagId);
                if (!object.Equals(fileTag, default(FileTags)))
                {
                    fileTagsRepository.Remove(fileTag);
                    uow.SaveChanges();
                }
            }
        }

        public void UpdateByKey(string key, string value)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var tag = GetByKey(key);
                if (tag != null)
                {
                    tag.Value = value;
                    uow.SaveChanges();
                }
            }
        }

        public void Add(int fileDoId, TagDO tag)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var tagRepository = uow.TagRepository;
                var fileTagRepository = uow.FileTagsRepository;
                var fileTag = new FileTags() { FileDoId = fileDoId };

                var dbTag = GetByKey(tag.Key);
                if (dbTag == null)
                {
                    tagRepository.Add(tag);
                    uow.SaveChanges();

                    dbTag = tagRepository.GetByKey(tag.Key);
                }

                fileTag.TagId = dbTag.Id;

                fileTagRepository.Add(fileTag);
                uow.SaveChanges();
            }
        }
         
    }
}
