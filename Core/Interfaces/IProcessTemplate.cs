using System.Collections.Generic;
using Data.Entities;

namespace Core.Interfaces
{
	public interface IProcessTemplate
	{
		IList<ProcessTemplateDO> GetForUser(string userId, bool isAdmin = false, int? id = null);

		int CreateOrUpdate(ProcessTemplateDO ptdo);
		void Delete(int id);
		void LaunchProcess(int curProcessTemplateId, EnvelopeDO curEnvelope);
	}
}