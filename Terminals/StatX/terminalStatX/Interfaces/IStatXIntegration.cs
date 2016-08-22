using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using terminalStatX.DataTransferObjects;

namespace terminalStatX.Interfaces
{
    public interface IStatXIntegration
    {
        Task<StatXAuthResponseDTO> Login(string clientName, string phoneNumber);

        Task<StatXAuthDTO> VerifyCodeAndGetAuthToken(string clientId, string phoneNumber, string verificationCode);

        Task<List<StatXGroupDTO>> GetGroups(StatXAuthDTO statXAuthDTO);

        Task<StatXGroupDTO> CreateGroup(StatXAuthDTO statXAuthDTO, string groupName);

        Task<List<BaseStatDTO>> GetStatsForGroup(StatXAuthDTO statXAuthDTO, string groupId);

        Task<BaseStatDTO> GetStat(StatXAuthDTO statXAuthDTO, string groupId, string statId);

        Task CreateStat(StatXAuthDTO statXAuthDTO, string groupId, BaseStatDTO statDTO);

        Task UpdateStatValue(StatXAuthDTO statXAuthDTO, string groupId, string statId, Dictionary<string, string> statValues, string title, string notes);
    }
}