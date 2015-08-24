using Data.Entities;

namespace Core.Interfaces
{
    public interface IProcess
    {
        ProcessDO Create(int processTemplateId, string envelopeId);
        void Launch(ProcessTemplateDO curProcessTemplate, DocuSignEventDO curEvent);
        void Execute(DocuSignEventDO curEvent, ProcessNodeDO curProcessNode);
    }
}