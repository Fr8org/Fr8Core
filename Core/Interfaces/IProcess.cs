using Data.Entities;

namespace Core.Interfaces
{
    public interface IProcess
    {
        ProcessDO Create(int processTemplateId, int envelopeId);
        void Launch(ProcessTemplateDO curProcessTemplate, EnvelopeDO curEnvelope);
        void Execute(EnvelopeDO curEnvelope, ProcessNodeDO curProcessNode);
    }
}