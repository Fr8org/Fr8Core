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

        Task<List<StatDTO>> GetStatsForGroup(StatXAuthDTO statXAuthDTO, string groupId);

        Task<StatDTO> GetStat(StatXAuthDTO statXAuthDTO, string groupId, string statId);

        Task UpdateStatValue(StatXAuthDTO statXAuthDTO, string groupId, string statId, Dictionary<string, string> statValues);
    }
}