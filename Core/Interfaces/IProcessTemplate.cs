using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;

namespace Core.Interfaces
{
	public interface IProcessTemplate
	{
		IList<ProcessTemplateDO> GetForUser(string userId, bool isAdmin = false, int? id = null);

		int CreateOrUpdate(IUnitOfWork uow, ProcessTemplateDO ptdo, bool withTemplate);
		void Delete(IUnitOfWork uow, int id);
        void LaunchProcess(IUnitOfWork uow, ProcessTemplateDO curProcessTemplate, CrateDTO curEventData = null);

        void MakeCollectionEqual<T>(IUnitOfWork uow, IList<T> collectionToUpdate, IList<T> sourceCollection) where T : class;

        IList<ProcessNodeTemplateDO> GetProcessNodeTemplates(ProcessTemplateDO curProcessTemplateDO);

        IList<ProcessTemplateDO> GetMatchingProcessTemplates(string userId, EventReportMS curEventReport);

        ActivityDO GetFirstActivity(ProcessTemplateDO curProcessTemplateDO);
	}
}