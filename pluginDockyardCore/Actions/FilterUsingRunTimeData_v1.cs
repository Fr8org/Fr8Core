using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Core.Interfaces;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Data.States;
using Data.States.Templates;
using Data.Wrappers;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;
using pluginDockyardCore.Interfaces;

namespace pluginDockyardCore.Actions
{
    public class FilterUsingRunTimeData_v1 : BasePluginAction
    {

        public FilterUsingRunTimeData_v1()
        {
        }

        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public ActionProcessResultDTO Execute(ActionDTO curActionDTO)
        {
            var actionDO = AutoMapper.Mapper.Map<ActionDO>(curActionDTO);

            // Get parent action-list.
            var curActionList = ((ActionListDO) actionDO.ParentActivity);

            if (!curActionList.ProcessID.HasValue)
            {
                throw new ApplicationException("Action.ActionList.ProcessID is empty.");
            }

            // Find crate with id "Criteria Filter Conditions".
            var curCrateStorage = actionDO.CrateStorageDTO();
            var curControlsCrate = curCrateStorage.CrateDTO
                .FirstOrDefault(x => x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME
                    && x.Label == "Configuration_Controls");

            if (curControlsCrate == null || string.IsNullOrEmpty(curControlsCrate.Contents))
            {
                throw new ApplicationException(string.Format("No crate found with Label == \"Configuration_Controls\" and ManifestType == \"{0}\"", CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME));
            }

            var controlsMS = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(curControlsCrate.Contents);
            
            var filterPaneControl = controlsMS.Controls.FirstOrDefault(x => x.Type == "filterPane");
            if (filterPaneControl == null)
            {
                throw new ApplicationException("No control found with Type == \"filterPane\"");
            }

            // Prepare envelope data.
            var curDocuSignEnvelope = new DocuSignEnvelope();
                // Should just change GetEnvelopeData to pass an EnvelopeDO.
            var curEnvelopeData = curDocuSignEnvelope.GetEnvelopeData(curDocuSignEnvelope);

            // Evaluate criteria using Contents json body of found Crate.
            var result = Evaluate(filterPaneControl.Value,
                curActionList.ProcessID.Value, curEnvelopeData);

            // Process result.
            if (result)
            {
                actionDO.ActionState = ActionState.Active;
            }
            else
            {
                curActionList.Process.ProcessState = ProcessState.Completed;
            }

            return new ActionProcessResultDTO() {Success = true};
        }

        private bool Evaluate(string criteria, int processId, IEnumerable<EnvelopeDataDTO> envelopeData)
        {
            if (criteria == null)
                throw new ArgumentNullException("criteria");
            if (criteria == string.Empty)
                throw new ArgumentException("criteria is empty", "criteria");
            if (envelopeData == null)
                throw new ArgumentNullException("envelopeData");

            return Filter(criteria, processId, envelopeData.AsQueryable()).Any();
        }

        private IQueryable<EnvelopeDataDTO> Filter(string criteria, int processId,
            IQueryable<EnvelopeDataDTO> envelopeData)
        {
            if (criteria == null)
                throw new ArgumentNullException("criteria");
            if (criteria == string.Empty)
                throw new ArgumentException("criteria is empty", "criteria");
            if (envelopeData == null)
                throw new ArgumentNullException("envelopeData");

            var filterDataDTO = JsonConvert.DeserializeObject<FilterDataDTO>(criteria);
            if (filterDataDTO.ExecutionType == FilterExecutionType.WithoutFilter)
            {
                return envelopeData;
            }
            else
            {
                EventManager.CriteriaEvaluationStarted(processId);

                var filterExpression = ParseCriteriaExpression(filterDataDTO.Conditions, envelopeData);
                var results = envelopeData.Provider.CreateQuery<EnvelopeDataDTO>(filterExpression);

                return results;
            }
        }

        private Expression ParseCriteriaExpression<T>(
            IEnumerable<FilterConditionDTO> conditions,
            IQueryable<T> queryableData)
        {
            var curType = typeof(T);

            Expression criteriaExpression = null;
            var pe = Expression.Parameter(curType, "p");

            foreach (var condition in conditions)
            {
                var propInfo = curType.GetProperty(condition.Field);
                var op = condition.Operator;
                var value = condition.Value;

                var leftExpr = Expression.Property(pe, propInfo);
                var rightExpr = Expression.Constant(value);

                Expression criterionExpression;
                switch (op)
                {
                    case "Equals":
                        criterionExpression = Expression.Equal(leftExpr, rightExpr);
                        break;
                    case "GreaterThan":
                        criterionExpression = Expression.GreaterThan(leftExpr, rightExpr);
                        break;
                    case "GreaterThanOrEquals":
                        criterionExpression = Expression.GreaterThanOrEqual(leftExpr, rightExpr);
                        break;
                    case "LessThan":
                        criterionExpression = Expression.LessThan(leftExpr, rightExpr);
                        break;
                    case "LessThanOrEquals":
                        criterionExpression = Expression.LessThanOrEqual(leftExpr, rightExpr);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("Not supported operator: {0}", op));
                }

                if (criteriaExpression == null)
                {
                    criteriaExpression = criterionExpression;
                }
                else
                {
                    criteriaExpression = Expression.AndAlso(criteriaExpression, criterionExpression);
                }
            }

            if (criteriaExpression == null)
            {
                criteriaExpression = Expression.Constant(true);
            }

            var whereCallExpression = Expression.Call(
                typeof (Queryable),
                "Where",
                new[] {typeof (T)},
                queryableData.Expression,
                Expression.Lambda<Func<T, bool>>(criteriaExpression, new[] {pe})
            );

            return whereCallExpression;
        }

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public CrateStorageDTO Configure(ActionDTO curActionDataPackageDTO)
        {
            return ProcessConfigurationRequest(curActionDataPackageDTO, ConfigurationEvaluator);
        }

        private CrateDTO CreateControlsCrate()
        {
            var fieldFilterPane = new FilterPaneFieldDefinitionDTO()
            {
                FieldLabel = "Criteria for Executing Actions",
                Type = "filterPane",
                Name = "Selected_Filter",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Queryable Criteria",
                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                }
            };

            return PackControlsCrate(fieldFilterPane);
        }

        /// <summary>
        /// Looks for first Create with Id == "Standard Design-Time" among all upcoming Actions.
        /// </summary>
        protected override CrateStorageDTO InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            if (curActionDTO.Id > 0)
            {
                //this conversion from actiondto to Action should be moved back to the controller edge
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    ActionDO curActionDO = _action.MapFromDTO(curActionDTO);

                    var curUpstreamFields = GetDesignTimeFields(curActionDO, GetCrateDirection.Upstream).Fields.ToArray();

                    //2) Pack the merged fields into a new crate that can be used to populate the dropdownlistbox
                    CrateDTO queryFieldsCrate = _crate.CreateDesignTimeFieldsCrate(
                        "Queryable Criteria", curUpstreamFields);
                    
                    //build a controls crate to render the pane
                    CrateDTO configurationControlsCrate = CreateControlsCrate();

                    var curCrates = new List<CrateDTO>
                    {
                        queryFieldsCrate,
                        configurationControlsCrate
                    };

                    return AssembleCrateStorage(curCrates);
                }
            }
            else
            {
                throw new ArgumentException(
                    "Configuration requires the submission of an Action that has a real ActionId");
            }
        }

        /// <summary>
        /// ConfigurationEvaluator always returns Initial,
        /// since Initial and FollowUp phases are the same for current action.
        /// </summary>
        private ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDataPackageDTO)
        {
            if (curActionDataPackageDTO.CrateStorage == null
                && curActionDataPackageDTO.CrateStorage.CrateDTO == null)
            {
                return ConfigurationRequestType.Initial;
            }


            var hasControlsCrate = curActionDataPackageDTO.CrateStorage.CrateDTO
                .Any(x => x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME
                    && x.Label == "Configuration_Controls");

            var hasQueryFieldsCrate = curActionDataPackageDTO.CrateStorage.CrateDTO
                .Any(x => x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                    && x.Label == "Queryable Criteria");

            if (hasControlsCrate && hasQueryFieldsCrate)
            {
                return ConfigurationRequestType.Followup;
            }
            else
            {
                return ConfigurationRequestType.Initial;
            }
        }
    }
}
