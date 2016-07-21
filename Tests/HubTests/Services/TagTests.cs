using Data.Entities;
using Data.Interfaces;
using Data.Utility;
using Data.Utility.JoinClasses;
using Hub.Interfaces;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Testing.Unit;

namespace HubTests.Services
{
    [TestFixture]
    [Category("TagService")]
    public class TagTests : BaseTest
    {
        private ITag _tagService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _tagService = ObjectFactory.GetInstance<ITag>();
        }

        private TagDO CreateTagDo(int id, string key, string value)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                TagDO tagDo = new TagDO()
                {
                    Id = id,
                    Key = key,
                    Value = value
                };

                uow.TagRepository.Add(tagDo);
                uow.SaveChanges();

                return tagDo;
            }
        }

        private FileDO CreateFileDo()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {    
                FileDO fileDo = new FileDO()
                {
                    Id = 1,
                    CloudStorageUrl = "test url",
                    OriginalFileName = "test_file_name.xls"
                };

                uow.FileRepository.Add(fileDo);
                uow.SaveChanges();

                return fileDo;
            }
        }

        private Dictionary<FileDO, TagDO> CreateFileTagsLink()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fileDo = CreateFileDo();
                var tagDo = CreateTagDo(1, "key1", "value1");
                FileTags fileTags = new FileTags()
                {
                    Id = 1,
                    FileDoId = fileDo.Id,
                    TagId = tagDo.Id
                };

                uow.FileTagsRepository.Add(fileTags);
                uow.SaveChanges();

                var dict = new Dictionary<FileDO, TagDO>();
                dict.Add(fileDo, tagDo);

                return dict;
            }
        }

        [Test]
        public void Tag_GetAllTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var testTag1 = CreateTagDo(1, "key1", "value1");
                var testTag2 = CreateTagDo(2, "key2", "value2");

                var listTags = _tagService.GetAll();

                Assert.IsTrue(listTags.Count == 2);
                var searchTag = listTags.FirstOrDefault(x => x.Key == testTag1.Key && x.Value == testTag1.Value);
                Assert.IsFalse(object.Equals(searchTag, default(TagDO)));

                searchTag = listTags.FirstOrDefault(x => x.Key == testTag2.Key && x.Value == testTag2.Value);
                Assert.IsFalse(object.Equals(searchTag, default(TagDO)));
            }
        }

        [Test]
        public void Tag_GetByIdTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var testTag = CreateTagDo(1, "key1", "value1");

                var searchTag = _tagService.GetById(testTag.Id);

                Assert.IsNotNull(searchTag);
                Assert.IsTrue(searchTag.Id == testTag.Id);
            }
        }

        [Test]
        public void Tag_GetByKeyTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var testTag = CreateTagDo(1, "key1", "value1");

                var searchTag = _tagService.GetByKey(testTag.Key);

                Assert.IsNotNull(searchTag);
                Assert.IsTrue(searchTag.Key == testTag.Key);
            }
        }

        [Test]
        public void Tag_GetListTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var dict = CreateFileTagsLink();

                var listTags = _tagService.GetList(dict.Keys.First().Id);

                Assert.IsTrue(listTags.Count == 1);
                Assert.IsTrue(listTags[0].Key == dict.Values.First().Key);
            }
        }

        [Test]
        public void Tag_GetFilesByTagTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var dict = CreateFileTagsLink();

                var listFiles = _tagService.GetFiles(dict.Values.First());

                Assert.IsTrue(listFiles.Count == 1);
                Assert.IsTrue(listFiles[0].Id == dict.Keys.First().Id);
            }
        }

        [Test]
        public void Tag_GetFilesByTagIdTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var dict = CreateFileTagsLink();

                var listFiles = _tagService.GetFiles(dict.Values.First().Id);

                Assert.IsTrue(listFiles.Count == 1);
                Assert.IsTrue(listFiles[0].Id == dict.Keys.First().Id);
            }
        }

        [Test]
        public void Tag_UpdateByKeyTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var testTag = CreateTagDo(1, "key1", "value1");

                _tagService.UpdateByKey(testTag.Key, "newvalue");
                var searchTag = uow.TagRepository.FindOne(x => x.Key == testTag.Key);

                Assert.IsNotNull(searchTag);
                Assert.IsTrue(searchTag.Value == "newvalue");
            }
        }

        [Test]
        public void Tag_UpdateAllTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var dict = CreateFileTagsLink();

                var tagDo = dict.Values.First();
                var fileDo = dict.Keys.First();
                var newTag = CreateTagDo(23, "testkey", "testvalue");

                tagDo.Value = "newvalue";

                _tagService.UpdateAll(fileDo.Id, new List<TagDO>() { tagDo, newTag });

                var listTags = new List<TagDO>();
                var fileTags = uow.FileTagsRepository.FindList(x => x.FileDoId == fileDo.Id);
                foreach (var fileTag in fileTags)
                {
                    listTags.Add(fileTag.Tag);
                }

                Assert.IsTrue(listTags.Count == 2);
                Assert.IsTrue(listTags[0].Value == "newvalue");
                Assert.IsTrue(listTags[1].Value == "testvalue");
            }
        }

        [Test]
        public void Tag_RemoveAllTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var dict = CreateFileTagsLink();
                var fileDo = dict.Keys.First();

                var listFileTags = uow.FileTagsRepository.FindList(x => x.FileDoId == fileDo.Id);
                Assert.IsTrue(listFileTags.Count() > 0);

                _tagService.RemoveAll(fileDo.Id);

                listFileTags = uow.FileTagsRepository.FindList(x => x.FileDoId == fileDo.Id);
                Assert.IsTrue(listFileTags.Count() == 0);
            }
        }

        [Test]
        public void Tag_RemoveTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var dict = CreateFileTagsLink();
                var fileDo = dict.Keys.First();
                var tagDo = dict.Values.First();

                var listFileTags = uow.FileTagsRepository.FindList(x => x.FileDoId == fileDo.Id);
                Assert.IsTrue(listFileTags.Count() > 0);

                _tagService.Remove(fileDo.Id, tagDo.Id);

                listFileTags = uow.FileTagsRepository.FindList(x => x.FileDoId == fileDo.Id);
                Assert.IsTrue(listFileTags.Count() == 0);
            }
        }

        [Test]
        public void Tag_AddTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fileDo = CreateFileDo();
                var tagDo = CreateTagDo(1, "key", "value");

                _tagService.Add(fileDo.Id, tagDo);

                var listTags = new List<TagDO>();
                var fileTags = uow.FileTagsRepository.FindList(x => x.FileDoId == fileDo.Id);
                foreach (var fileTag in fileTags)
                {
                    listTags.Add(fileTag.Tag);
                }

                Assert.IsTrue(listTags.Count == 1);
                Assert.IsTrue(listTags[0].Value == "value");
                Assert.IsTrue(listTags[0].Key == "key");
            }
        }

    }
}
