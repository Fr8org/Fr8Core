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
    public class HistoryItemDO : BaseObject, IHistoryItemDO
    {
        [Key]
        public int Id { get; set; }
        public string ObjectId { get; set; }      
        public string Fr8UserId { get; set; }
        public string PrimaryCategory { get; set; }
        public string SecondaryCategory { get; set; }
        public string Component { get; set; }
        public string Activity { get; set; }
        public string Data { get; set; }
        public string Status { get; set; }

        public override void BeforeSave()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var a = ObjectFactory.Container.WhatDoIHave();
                var configRepo = ObjectFactory.GetInstance<IConfigRepository>();
                string customerAddress = null;

                Fr8AccountDO acct = uow.UserRepository.GetByKey(Fr8UserId);
                if(acct != null && acct.EmailAddress != null)
                {
                    customerAddress = acct.EmailAddress.Address;
                }

                string dataHeader="";
                if (!string.IsNullOrEmpty(customerAddress))
                {
                    dataHeader = string.Format(
                        "EmailAddress: {1} ",
                        PrimaryCategory,
                        customerAddress
                    );    
                }

                Data = dataHeader + "\r\n" + Data;

                if (configRepo.Get("LogLevel", String.Empty) == "Verbose")
                    Logger.GetLogger().Info(Data);
            }
        }
    }
}
