using AutoMapper;
using Data.Control;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Crates;
using Utilities.Configuration.Azure;
using Utilities.Logging;

namespace Hub.Managers
{
    public class RouteManager
    {
        private readonly IActivityTemplate _activityTemplate;
        private readonly IActivity _activity;
        private readonly ICrateManager _crateManager;
        

        public RouteManager()
        {
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _activity = ObjectFactory.GetInstance<IActivity>();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        public async Task CreateRoute_LogFr8InternalEvents(string curFr8UserId)
        {
            try
            {


                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var curFr8Account = uow.UserRepository.GetOrCreateUser(curFr8UserId);

                    //check if the plan already created

                    var existingRoute = GetExistingRoute(uow, "LogFr8InternalEvents", curFr8Account.Email);


                    if (existingRoute != null)
                    {
                        //if plan is already created, just make it active and return
                        existingRoute.RouteState = RouteState.Active;
                        uow.SaveChanges();
                        return;
                    }

                    //Create a plan
                    PlanDO plan = new PlanDO
                    {
                        Name = "LogFr8InternalEvents",
                        Description = "Log Fr8Internal Events",
                        Fr8Account = curFr8Account,
                        RouteState = RouteState.Active,
                        Tag = "Monitor",
                        Id = Guid.NewGuid(),
                    };

                    //create a sub plan
                    var subroute = new SubrouteDO(true)
                    {
                        Id = Guid.NewGuid(),
                    };

                    //update Route and Sub plan into database
                    plan.ChildNodes = new List<RouteNodeDO> { subroute };

                    uow.PlanRepository.Add(plan);

                    uow.SaveChanges();

                    //get activity templates of required actions
                    var activity1 = Mapper.Map<ActivityTemplateDTO>(_activityTemplate.GetByName(uow, "Monitor_Fr8_Events_v1"));
                    var activity2 = Mapper.Map<ActivityTemplateDTO>(_activityTemplate.GetByName(uow, "SaveToFr8Warehouse_v1"));

                    //create and configure required actions
                    await _activity.CreateAndConfigure(uow, curFr8Account.Id, activity1.Id, activity1.Label, null, subroute.Id);

                    var result = await _activity.CreateAndConfigure(uow, curFr8Account.Id, activity2.Id, activity2.Label, null, subroute.Id);

                    if (result is ActivityDO)
                    {
                        var activityDTO = Mapper.Map<ActivityDTO>(result);
                        SetSelectedCrates(activityDTO);

                        var activityDO = _activity.MapFromDTO(activityDTO);
                        await _activity.Configure(uow, curFr8Account.Id, activityDO);
                    }

                    //update database
                    uow.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().Error("Error while creating the Fr8 monitor Route " + e.Message, e);
                throw;
            }
        }

        private PlanDO GetExistingRoute(IUnitOfWork uow, string routeName, string fr8AccountEmail)
        {
            if (uow.PlanRepository.GetPlanQueryUncached().Any(existingRoute =>
                existingRoute.Name.Equals(routeName) &&
                existingRoute.Fr8Account.Email.Equals(fr8AccountEmail)))
            {
                return uow.PlanRepository.GetPlanQueryUncached().First(existingRoute =>
                    existingRoute.Name.Equals(routeName) &&
                    existingRoute.Fr8Account.Email.Equals(fr8AccountEmail));
            }

            return null;
        }

        public void SetSelectedCrates(ActivityDTO storeMTDataActivity)
        {
            if (storeMTDataActivity.CrateStorage != null)
            {
                using (var updater = _crateManager.UpdateStorage(() => storeMTDataActivity.CrateStorage))
                {
                    var configControlCM = updater.CrateStorage
                        .CrateContentsOfType<StandardConfigurationControlsCM>()
                        .First();

                    var upstreamCrateChooser = (UpstreamCrateChooser)configControlCM.FindByName("UpstreamCrateChooser");
                    var existingDdlbSource = upstreamCrateChooser.SelectedCrates[0].ManifestType.Source;
                    var existingLabelDdlb = upstreamCrateChooser.SelectedCrates[0].Label;

                    var routeActivated = new DropDownList
                    {
                        selectedKey = "RouteActivated",
                        Value = "13",
                        Name = "UpstreamCrateChooser_mnfst_dropdown_0",
                        Source = existingDdlbSource
                    };
                    var routeDeactivated = new DropDownList
                    {
                        selectedKey = "RouteDeactivated",
                        Value = "13",
                        Name = "UpstreamCrateChooser_mnfst_dropdown_1",
                        Source = existingDdlbSource
                    };
                    var containerLaunched = new DropDownList
                    {
                        selectedKey = "ContainerLaunched",
                        Value = "13",
                        Name = "UpstreamCrateChooser_mnfst_dropdown_2",
                        Source = existingDdlbSource
                    };

                    var containerExecutionComplete = new DropDownList
                    {
                        selectedKey = "ContainerExecutionComplete",
                        Value = "13",
                        Name = "UpstreamCrateChooser_mnfst_dropdown_3",
                        Source = existingDdlbSource
                    };

                    var actionExecuted = new DropDownList
                    {
                        selectedKey = "ActionExecuted",
                        Value = "13",
                        Name = "UpstreamCrateChooser_mnfst_dropdown_4",
                        Source = existingDdlbSource
                    };


                    upstreamCrateChooser.SelectedCrates = new List<CrateDetails>()
                {
                    new CrateDetails { ManifestType = routeActivated, Label = existingLabelDdlb },
                    new CrateDetails { ManifestType = routeDeactivated, Label = existingLabelDdlb },
                    new CrateDetails { ManifestType = containerLaunched, Label = existingLabelDdlb },
                    new CrateDetails { ManifestType = containerExecutionComplete, Label = existingLabelDdlb },
                    new CrateDetails { ManifestType = actionExecuted, Label = existingLabelDdlb }
                };
                }
            }
        }
    }
}