using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Utilities;

namespace terminalQuickBooks.Services
{
    public static class ObjectMapper
    {
        /// <summary>
        /// This method perfoms mapping from StandardPayloadDataCM to StandardAccountingTransactionCM
        /// Please refer to the https://maginot.atlassian.net/wiki/display/SH/Manifest+%3A+Standard+Payload+Data
        /// </summary>
        /// <param name="payloadDataCrate"></param>
        /// <returns></returns>
        public static StandardAccountingTransactionCM MapPayloadDataToAccountingTransactionCM(
        StandardPayloadDataCM payloadDataCrate)
        {
            if (payloadDataCrate == null)
            {
                throw new ArgumentNullException("No StandardPayloadDataCM object provided");
            }
            //The Object Type field should be "StandardPayloadDataCM"
            var curObjectType = payloadDataCrate.ObjectType;
            if (!string.Equals(curObjectType, "StandardPayloadDataCM") || curObjectType.IsNullOrEmpty())
            {
                throw new ArgumentException("ObjectType is incorrect");
            }
            //Start Mapping of the fields
            var curPayloadObjectList = payloadDataCrate.PayloadObjects;
            //The objects list should not be empty
            if (curPayloadObjectList.Count == 0)
            {
                throw new Exception("No PayloadObjectDTO is provided");
            }
            //Get number of elements to iterate through
            var curNumOfElements = curPayloadObjectList.Count;
            //The first object should be the StandardAccountingDTO with Name, Date and Memo fields
            var curFirstPayloadObjectDTO = curPayloadObjectList[0];
            //First FieldDTO value is Name
            var curAccountTransName = curFirstPayloadObjectDTO.PayloadObject.Find(l => l.Key == "Name").Value;
            //Second FieldDTO value is Date
            var curAccountTransDate = curFirstPayloadObjectDTO.PayloadObject.Find(l => l.Key == "TransactionDate").Value;
            //Third FieldDTO value is Memo
            var curAccountTransMemo = curFirstPayloadObjectDTO.PayloadObject.Find(l => l.Key == "Memo").Value;
            //Upcomming PayloadObjectDTO are to be FinancialLineDTOs
            //with the following fields: Amount, AccountName, AccountId, DebitOrCredit, Description
            //Begin iteration from the second element
            var curFinLineDTOList = new List<FinancialLineDTO>();
            for (int i = 1; i < curNumOfElements; i++)
            {
                var curFinLineDTOToAdd = MapPayloadObjectToFinancialLineDTO(curPayloadObjectList[i]);
                curFinLineDTOList.Add(curFinLineDTOToAdd);
            }
            //Pack DTO
            var curStadardAccountingTransactionDTO = new StandardAccountingTransactionDTO()
            {
                TransactionDate = DateTime.Parse(curAccountTransDate),
                Memo = curAccountTransMemo,
                Name = curAccountTransName,
                FinancialLines = curFinLineDTOList

            };
            //Return crate
            return new StandardAccountingTransactionCM()
            {
                AccountingTransactionDTO = curStadardAccountingTransactionDTO
            };
        }
        /// <summary>
        /// Helps MapPayloadDataToAccountingTransactionCM() method
        /// </summary>
        /// <param name="payloadObjectDTO"></param>
        /// <returns></returns>
        public static FinancialLineDTO MapPayloadObjectToFinancialLineDTO(PayloadObjectDTO payloadObjectDTO)
        {
            //Map Amount
            var curFinLineDTOAmount = payloadObjectDTO.PayloadObject.Find(l => l.Key == "Amount").Value;
            //Map AccountName
            var curFinLineDTOAccountName = payloadObjectDTO.PayloadObject.Find(l => l.Key == "AccountName").Value;
            //Map AccountId
            var curFinLineDTOAccountId = payloadObjectDTO.PayloadObject.Find(l => l.Key == "AccountId").Value;
            //Map DebitOrCredit
            var curFinLineDTODebitOrCredit = payloadObjectDTO.PayloadObject.Find(l => l.Key == "DebitOrCredit").Value;
            //Map Description
            var curFinLineDTODescription = payloadObjectDTO.PayloadObject.Find(l => l.Key == "Description").Value;
            return new FinancialLineDTO()
            {
                AccountId = curFinLineDTOAccountId,
                AccountName = curFinLineDTOAccountName,
                Amount = curFinLineDTOAmount,
                DebitOrCredit = curFinLineDTODebitOrCredit,
                Description = curFinLineDTODescription
            };
        }
    }
}