using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Data.Infrastructure.AutoMapper;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Services;
using terminalFr8Core.Actions;
using terminalFr8Core.Activities;

[assembly: OwinStartup(typeof(terminalFr8Core.Startup))]

namespace terminalFr8Core
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
            ConfigureProject(selfHost, Fr8CoreStructureMapConfiguration.LiveConfiguration);

            Container.Configure(x => x.AddRegistry<Hub.StructureMap.StructureMapBootStrapper.LiveMode>());
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
            ActivityStore.RegisterActivity<Add_Payload_Manually_v1>(Add_Payload_Manually_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<App_Builder_v1>(App_Builder_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Build_Message_v1>(Build_Message_v1.ActivityTemplateDTO);
            //ActivityStore.RegisterActivity<Build_Query_v1>(Build_Query_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Connect_To_Sql_v1>(Connect_To_Sql_v1.ActivityTemplateDTO);
            //ActivityStore.RegisterActivity<Convert_Related_Fields_Into_Table_v1>(Convert_Related_Fields_Into_Table_v1.ActivityTemplateDTO); FR-4669
            ActivityStore.RegisterActivity<Execute_Sql_v1>(Execute_Sql_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Extract_Table_Field_v1>(Extract_Table_Field_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Filter_Object_List_By_Incoming_Message_v1>(Filter_Object_List_By_Incoming_Message_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Get_Data_From_Fr8_Warehouse_v1>(Get_Data_From_Fr8_Warehouse_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Get_File_From_Fr8_Store_v1>(Get_File_From_Fr8_Store_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Loop_v1>(Loop_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Monitor_Fr8_Events_v1>(Monitor_Fr8_Events_v1.ActivityTemplateDTO);
            //ActivityStore.RegisterActivity<Query_Fr8_Warehouse_v1>(Query_Fr8_Warehouse_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Save_To_Fr8_Warehouse_v1>(Save_To_Fr8_Warehouse_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Search_Fr8_Warehouse_v1>(Search_Fr8_Warehouse_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Select_Fr8_Object_v1>(Select_Fr8_Object_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Set_Delay_v1>(Set_Delay_v1.ActivityTemplateDTO);
            //ActivityStore.RegisterActivity<Show_Report_Onscreen_v1>(Show_Report_Onscreen_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Store_File_v1>(Store_File_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Make_A_Decision_v1>(Make_A_Decision_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Test_Incoming_Data_v1>(Test_Incoming_Data_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Send_Email_v1>(Send_Email_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Send_SMS_v1>(Send_SMS_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Save_All_Payload_To_Fr8_Warehouse>(Save_All_Payload_To_Fr8_Warehouse.ActivityTemplateDTO);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActivityController),
                    typeof(Controllers.EventController),
                    typeof(Controllers.TerminalController)
                };
        }
    }
}
