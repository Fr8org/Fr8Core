using System;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;

namespace Core.Services
{
	public class Process: IProcess
	{
		private readonly IProcessNode _processNode;

		public Process()
		{
			this._processNode = ObjectFactory.GetInstance< IProcessNode >();
		}

		/// <summary>
		/// New Process object
		/// </summary>
		/// <param name="processTemplateId"></param>
		/// <param name="envelopeId"></param>
		/// <returns></returns>
		public ProcessDO Create( int processTemplateId, int envelopeId )
		{
			var curProcess = new ProcessDO();
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				var template = uow.ProcessTemplateRepository.GetByKey( processTemplateId );
				var envelope = uow.EnvelopeRepository.GetByKey( envelopeId );

				if( template == null )
					throw new ArgumentNullException( "processTemplateId" );
				if( envelope == null )
					throw new ArgumentNullException( "envelopeId" );

				curProcess.Name = template.Name;
				curProcess.ProcessState = ProcessState.Processing;
				curProcess.EnvelopeId = envelopeId.ToString();

				var processNode = this._processNode.Create( uow, curProcess );
				uow.SaveChanges();

				curProcess.CurrentProcessNodeId = processNode.Id;

				uow.ProcessRepository.Add( curProcess );
				uow.SaveChanges();
			}
			return curProcess;
		}

		public void Execute( ProcessTemplateDO curProcessTemplate, EnvelopeDO curEnvelope )
		{
			var curProcess = this.Create( curProcessTemplate.Id, curEnvelope.Id );
			if( curProcess.ProcessState == ProcessState.Failed && curProcess.ProcessState == ProcessState.Completed )
				return;			
			
			curProcess.ProcessState = ProcessState.Processing;
			using( var uow = ObjectFactory.GetInstance< IUnitOfWork >() )
			{
				ProcessNodeDO curProcessNode;
				if( curProcess.CurrentProcessNodeId == 0 )
				{
					var curProcessNodeTemplate = uow.ProcessNodeTemplateRepository.GetByKey( curProcessTemplate.StartingProcessNodeTemplate );
					curProcessNode = new ProcessNodeDO();
					curProcessNode.Name = curProcessNodeTemplate.Name;
					curProcessNode.ParentProcessId = curProcess.Id;
				}
				curProcessNode = uow.ProcessNodeRepository.GetByKey( curProcess.CurrentProcessNodeId );

				this._processNode.Execute( curProcess, curEnvelope, curProcessNode );
			}
		}
	}
}