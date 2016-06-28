using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;

namespace terminalMailChimp.Activities
{
    public class Update_List_v1 : TerminalActivity<Update_List_v1.ActivityUi>
    {
        public Update_List_v1(ICrateManager crateManager) : base(crateManager)
        {
        }

        public class ActivityUi : StandardConfigurationControlsCM
        {

        }

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Update_List",
            Label = "Update List",
            Version = "1",
            Category = ActivityCategory.Forwarders,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            WebService = TerminalData.WebServiceDTO,
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public override Task Initialize()
        {
            throw new NotImplementedException();
        }

        public override Task FollowUp()
        {
            throw new NotImplementedException();
        }

        public override Task Run()
        {
            throw new NotImplementedException();
        }
    }
}