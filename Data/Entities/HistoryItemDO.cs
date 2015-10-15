using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using Utilities.Logging;
using StructureMap;
using Utilities;

namespace Data.Entities
{
    public class HistoryItemDO : BaseDO, IHistoryItemDO
    {
        [Key]
        public int Id { get; set; }
        public string ObjectId { get; set; }      
        public string CustomerId { get; set; }
        public string PrimaryCategory { get; set; }
        public string SecondaryCategory { get; set; }
        public string Activity { get; set; }
        public string Data { get; set; }
        public string Status { get; set; }

        public override void BeforeSave()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var configRepo = ObjectFactory.GetInstance<IConfigRepository>();
                string customerAddress;

                Fr8AccountDO acct = uow.UserRepository.GetByKey(CustomerId);
                if(acct != null && acct.EmailAddress != null)
                {
                    customerAddress = acct.EmailAddress.Address;
                }
                else
                {
                    customerAddress = "<unknown>";
                }
              
                Data = string.Format("{0} ID :{1}, EmailAddress: {2} ", PrimaryCategory, ObjectId, (CustomerId == null ? "" : customerAddress)) + Data;

                if (configRepo.Get("LogLevel", String.Empty) == "Verbose")
                    Logger.GetLogger().Info(Data);
            }
        }
    }
}
