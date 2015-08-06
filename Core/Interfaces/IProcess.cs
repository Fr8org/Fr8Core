using Data.Entities;

namespace Core.Interfaces
{
    public interface IProcess
    {
        ProcessDO Create(int processTemplateId, int envelopeId);
        void Execute(ProcessTemplateDO curProcessTemplate, EnvelopeDO curEnvelope);
    }
}