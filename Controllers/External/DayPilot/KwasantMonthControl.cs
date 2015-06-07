using System;
using System.Linq.Expressions;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Month;
using DayPilot.Web.Mvc.Events.Common;
using KwasantWeb.Controllers.External.DayPilot.Providers;
using Utilities;

namespace KwasantWeb.Controllers.External.DayPilot
{
    public class KwasantMonthController : DayPilotMonth
    {
        private readonly IEventDataProvider _provider;
        public KwasantMonthController(IEventDataProvider provider)
        {
            _provider = provider;
        }

        protected override void OnEventBubble(EventBubbleArgs e)
        {
            e.BubbleHtml = _provider.GetTimeslotBubbleText(int.Parse(e.Id));
        }

        protected override void OnCommand(CommandArgs e)
        {
            switch (e.Command)
            {
                case "navigate":
                    if (e.Data["day"] != null)
                    {
                        StartDate = (DateTime)e.Data["day"];
                    }
                    Update(CallBackUpdateType.Full);
                    break;

                case "refresh":
                    Update();
                    break;

                case "selected":
                    if (SelectedEvents.Count > 0)
                    {
                        EventInfo ei = SelectedEvents[0];
                        SelectedEvents.RemoveAt(0);
                        UpdateWithMessage("Event removed from selection: " + ei.Text);
                    }

                    break;
            }
        }


        protected override void OnFinish()
        {
            // only load the data if an update was requested by an Update() call
            if (UpdateType == CallBackUpdateType.None)
            {
                return;
            }

            var reflectionHelper = new ReflectionHelper<DayPilotTimeslotInfo>();
            DataStartField = reflectionHelper.GetPropertyName(ev => ev.StartDate);
            DataEndField = reflectionHelper.GetPropertyName(ev => ev.EndDate);
            DataTextField = reflectionHelper.GetPropertyName(ev => ev.Text);
            DataIdField = reflectionHelper.GetPropertyName(ev => ev.Id);
            DataTagFields = reflectionHelper.GetPropertyName(ev => ev.Tag);

            Events = _provider.LoadData();
        }

        protected override void OnBeforeEventRender(BeforeEventRenderArgs e)
        {
            _provider.BeforeCellRender(e);
            base.OnBeforeEventRender(e);
        }

    }
}