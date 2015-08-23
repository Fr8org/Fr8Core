using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace Core.Interfaces
{
	public interface IProcessTemplate
	{
		IList<ProcessTemplateDO> GetForUser(string userId, bool isAdmin = false, int? id = null);

		int CreateOrUpdate(IUnitOfWork uow, ProcessTemplateDO ptdo, bool withTemplate);
		void Delete(IUnitOfWork uow, int id);
        void LaunchProcess(IUnitOfWork uow, ProcessTemplateDO curProcessTemplate, EnvelopeDO curEnvelope);
        void MakeCollectionEqual<T>(IList<T> collectionToUpdate, IList<T> sourceCollection);
    }
}