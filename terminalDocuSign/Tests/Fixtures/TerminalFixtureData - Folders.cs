using System;
using System.Collections.Generic;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign.Tests.Fixtures
{
    public partial class TerminalFixtureData
    {
        public static List<DocusignFolderInfo> GetFolders()
        {
            return new List<DocusignFolderInfo> 
            {
                new DocusignFolderInfo
                {
                    FolderId = "folder_1",
                    Name = "Folder 1",
                },

                new DocusignFolderInfo
                {
                    FolderId = "folder_2",
                    Name = "Folder 2",
                },
            };
        }
        
        public static List<FolderItem> GetFolderItems(string folderName, int seed, int count, string status)
        {
            var items = new List<FolderItem>(count);
            var prefix = "[" + folderName + "]";

            for (int j = 0, i = seed; j < count; j++, i++)
            {
                items.Add( new FolderItem
                {
                    Name =  prefix + "Envelope "+i,
                    EnvelopeId = prefix +"Envelope "+i,
                    OwnerName = prefix +"Owner "+(i%5),
                    PageCount = 1,
                    SenderName = prefix + "Sender " + (i % 10),
                    SenderEmail = prefix + "Sender" + (i % 10)+ "@mail" ,
                    Status = status,
                    Subject = prefix + "Sender " + (i % 10),
                    CompletedDateTime = new DateTime(2015, 10, 2, 1, 1 ,1),
                    CreatedDateTime =  new DateTime(2015, 10, 1, 1, 1 ,1),
                });
            }

            return items;
        }


        public static List<FolderItem> GetFolderInfo(string folderName)
        {
            var prefix = "[" + folderName + "]";

            return new List<FolderItem>
            {
                new FolderItem
                {
                    Name =  prefix + "Envelope 1",
                    EnvelopeId = prefix +"Envelope 1",
                    OwnerName = prefix +"Owner 1",
                    PageCount = 1,
                    SenderName = prefix +"Sender 1",
                    SenderEmail = prefix + "Sender1@mail" ,
                    Status = "Sent",
                    Subject =  prefix + "Sender 1",
                    CompletedDateTime = new DateTime(2015, 10, 2, 1, 1 ,1),
                    CreatedDateTime =  new DateTime(2015, 10, 1, 1, 1 ,1),
                } ,
                new FolderItem
                {
                    Name =  prefix + "Envelope 2",
                    EnvelopeId = prefix +"Envelope 2",
                    OwnerName = prefix +"Owner 2",
                    PageCount = 1,
                    SenderName = prefix +"Sender 2",
                    SenderEmail = prefix + "Sender2@mail",
                    Status = "Sent",
                    Subject =  prefix + "Sender 2",
                    CompletedDateTime = new DateTime(2015, 10, 2, 1, 1 ,1),
                    CreatedDateTime =  new DateTime(2015, 10, 1, 1, 1 ,1),
                } 
            };
        }
    }
}