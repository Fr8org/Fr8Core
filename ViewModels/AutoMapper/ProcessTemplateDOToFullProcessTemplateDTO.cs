using AutoMapper;
using Data.Interfaces;
using Data.Migrations;

namespace Web.ViewModels.AutoMapper
{
    public class ProcessTemplateDOToFullProcessTemplateDTO
        : ITypeConverter<ProcessTemplateDO, FullProcessTemplateDTO>
    {
        public const string UnitOfWork_OptionsKey = "UnitOfWork";


        public FullProcessTemplateDTO Convert(ResolutionContext context)
        {
            //var uow = (IUnitOfWork)context.Options.Items[UnitOfWork_OptionsKey];
            throw new System.NotImplementedException();
        }
    }
}