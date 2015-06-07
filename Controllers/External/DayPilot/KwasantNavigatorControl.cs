using System;
using System.Linq.Expressions;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Events.Navigator;
using KwasantWeb.Controllers.External.DayPilot.Providers;
using Utilities;

namespace KwasantWeb.Controllers.External.DayPilot
{
    public class KwasantNavigatorControl : DayPilotNavigator
    {        
        private readonly IEventDataProvider _provider;

        public KwasantNavigatorControl(IEventDataProvider provider)
        {
            _provider = provider;
        }

        protected override void OnVisibleRangeChanged(VisibleRangeChangedArgs a)
        {
            var reflectionHelper = new ReflectionHelper<DayPilotTimeslotInfo>();
            DataStartField = reflectionHelper.GetPropertyName(ev => ev.StartDate);
            DataEndField = reflectionHelper.GetPropertyName(ev => ev.EndDate);
            DataIdField = reflectionHelper.GetPropertyName(ev => ev.Id);

            Events = _provider.LoadData();
        }


    }
}