using System.Linq;
using Data.Entities;

namespace Core.Interfaces
{
    public interface IProcessTemplate 
    {
        IQueryable<ProcessTemplateDO> GetForUser(string userId, int? id = null);

        int CreateOrUpdate(ProcessTemplateDO ptdo);
        void Delete(int id); 
		void LaunchProcess( int curProcessTemplateId, EnvelopeDO curEnvelope );
    }

   
}