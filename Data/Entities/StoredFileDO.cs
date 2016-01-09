using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Entities
{
    public class StoredFileDO : BaseObject, ISaveHook
    {
        [Key]
        public int Id { get; set; }

        public String OriginalName { get; set; }
        public String StoredName { get; set; }

        private Stream _data;

        public Stream GetData()
        {
            if (_data == null)
                FileManager.LoadFile(this);
            _data.Position = 0;
            
            var memStr = new MemoryStream();
            _data.CopyTo(memStr);
            memStr.Position = 0;

            return memStr;
        }
        public void SetData(Stream value)
        {
            _data = value;
        }
        
        [NotMapped]
        public String StringData
        {
            get
            {
                return new StreamReader(GetData()).ReadToEnd();
            }
            set
            {
                MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(value));
                SetData(memStream);
            }
        }

        [NotMapped]
        public byte[] Bytes
        {
            get
            {
                MemoryStream memoryStream = new MemoryStream();
                GetData().CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
            set
            {
                SetData(new MemoryStream(value));
            }
        }

        public override void BeforeSave()
        {
            FileManager.SaveFile(this);
            base.BeforeSave();
        }
    }
}
