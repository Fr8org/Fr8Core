using Data.Entities;

namespace Core.Interfaces
{
    public interface IProcess
    {
        ProcessDO Create(int processTemplateId, int envelopeId);
        void Launch(ProcessTemplateDO curProcessTemplate, DocuSignEventDO curEvent);
        void Execute(DocuSignEventDO curEvent, ProcessNodeDO curProcessNode);
    }
}