using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Data.Infrastructure.AutoMapper;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;
using Microsoft.Owin;
using Owin;
using StructureMap;
using terminalDocuSign;
using terminalDocuSign.Controllers;
using terminalDocuSign.Actions;
using terminalDocuSign.Activities;

[assembly: OwinStartup(typeof(Startup))]

namespace terminalDocuSign
{
    public class Startup : BaseConfiguration
    {
        public Startup()
            : base(TerminalData.TerminalDTO)
        {
        }

        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, TerminalDocusignStructureMapBootstrapper.LiveConfiguration);
            Container.Configure(Hub.StructureMap.StructureMapBootStrapper.LiveConfiguration);

            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            SwaggerConfig.Register(_configuration);
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();
            app.UseWebApi(_configuration);
            if (!selfHost)
            {
                StartHosting();
            }
        }

        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<Monitor_DocuSign_Envelope_Activity_v1>(Monitor_DocuSign_Envelope_Activity_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Send_DocuSign_Envelope_v1>(Send_DocuSign_Envelope_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Send_DocuSign_Envelope_v2>(Send_DocuSign_Envelope_v2.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Use_DocuSign_Template_With_New_Document_v1>(Use_DocuSign_Template_With_New_Document_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Prepare_DocuSign_Events_For_Storage_v1>(Prepare_DocuSign_Events_For_Storage_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Mail_Merge_Into_DocuSign_v1>(Mail_Merge_Into_DocuSign_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Extract_Data_From_Envelopes_v1>(Extract_Data_From_Envelopes_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Track_DocuSign_Recipients_v1>(Track_DocuSign_Recipients_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Track_DocuSign_Recipients_v2>(Track_DocuSign_Recipients_v2.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Query_DocuSign_v1>(Query_DocuSign_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Query_DocuSign_v2>(Query_DocuSign_v2.ActivityTemplateDTO);
            //ActivityStore.RegisterActivity<Search_DocuSign_History_v1>(Search_DocuSign_History_v1.ActivityTemplateDTO); FR-4750, reason in in activity class
            ActivityStore.RegisterActivity<Get_DocuSign_Envelope_v1>(Get_DocuSign_Envelope_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Get_DocuSign_Template_v1>(Get_DocuSign_Template_v1.ActivityTemplateDTO);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(ActivityController),
                    typeof(EventController),
                    typeof(TerminalController),
                    typeof(AuthenticationController)
                };
        }
    }
}
