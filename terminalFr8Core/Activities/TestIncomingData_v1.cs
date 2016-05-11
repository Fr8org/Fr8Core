using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Data.Entities;
using Data.Infrastructure;
using Data.States;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class TestIncomingData_v1 : BaseTerminalActivity
    {

        public TestIncomingData_v1()
        {
        }

        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public virtual async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);

            var controlsMS = GetControlsManifest(curActivityDO);

            ControlDefinitionDTO filterPaneControl = controlsMS.Controls.FirstOrDefault(x => x.Type == ControlTypes.FilterPane);
            if (filterPaneControl == null)
            {
                return Error(curPayloadDTO, "No control found with Type == \"filterPane\"");
            }

            var curValues = GetAllPayloadFields(curPayloadDTO);
            // Prepare envelope data.

            // Evaluate criteria using Contents json body of found Crate.
            bool result = false;
            try
            {
                result = Evaluate(filterPaneControl.Value, curPayloadDTO.ContainerId, curValues);
            }
            catch (Exception e)
            {
            }

            if (!result)
            {
                return TerminateHubExecution(curPayloadDTO);
            }

            return Success(curPayloadDTO);
        }

        protected List<FieldDTO> GetAllPayloadFields(PayloadDTO curPayloadDTO)
        {
            var valuesCrates = CrateManager.FromDto(curPayloadDTO.CrateStorage).CrateContentsOfType<StandardPayloadDataCM>();
            var curValues = new List<FieldDTO>();

            foreach (var valuesCrate in valuesCrates)
            {
                curValues.AddRange(valuesCrate.AllValues());
            }

            return curValues;
        }

        private bool Evaluate(string criteria, Guid processId, IEnumerable<FieldDTO> values)
        {
            if (criteria == null)
                throw new ArgumentNullException("criteria");
            if (criteria == string.Empty)
                throw new ArgumentException("criteria is empty", "criteria");
            if (values == null)
                throw new ArgumentNullException("envelopeData");

            return Filter(criteria, processId, values.AsQueryable()).Any();
        }

        private IQueryable<FieldDTO> Filter(string criteria,
            Guid processId, IQueryable<FieldDTO> values)
        {
            if (criteria == null)
                throw new ArgumentNullException("criteria");
            if (criteria == string.Empty)
                throw new ArgumentException("criteria is empty", "criteria");
            if (values == null)
                throw new ArgumentNullException("envelopeData");

            var filterDataDTO = JsonConvert.DeserializeObject<FilterDataDTO>(criteria);
            if (filterDataDTO.ExecutionType == FilterExecutionType.WithoutFilter)
            {
                return values;
            }
            else
            {
                EventManager.CriteriaEvaluationStarted(processId);

                var filterExpression = ParseCriteriaExpression(filterDataDTO.Conditions, values);
                var results = values.Provider.CreateQuery<FieldDTO>(filterExpression);

                return results;
            }
        }
        public static int Compare(object left, object right)
        {
            if (left == null && right == null)
            {
                return 0;
            }
            if (left is string && right is string)
            {
                decimal v1;
                decimal v2;
                if (decimal.TryParse((string)left, out v1) && decimal.TryParse((string)right, out v2))
                {
                    return v1.CompareTo(v2);
                }
                return string.Compare((string)left, (string)right, StringComparison.Ordinal);
            }
            return -2;
        }
        protected Expression ParseCriteriaExpression(IEnumerable<FilterConditionDTO> conditions, IQueryable<FieldDTO> queryableData)
        {
            var curType = typeof(FieldDTO);
            Expression criteriaExpression = null;
            var pe = Expression.Parameter(curType, "p");
            foreach (var condition in conditions)
            {
                var namePropInfo = curType.GetProperty("Key");
                var valuePropInfo = curType.GetProperty("Value");
                var nameLeftExpr = Expression.Property(pe, namePropInfo);
                var nameRightExpr = Expression.Constant(condition.Field);
                var nameExpression = Expression.Equal(nameLeftExpr, nameRightExpr);
                //var valueLeftExpr = Expression.Invoke(TryMakeDecimalExpression.Value, Expression.Property(pe, valuePropInfo));
              //  var valueRightExpr = Expression.Invoke(TryMakeDecimalExpression.Value, Expression.Constant(condition.Value));
               // var comparisionExpr = Expression.Call(valueLeftExpr, "CompareTo", null, valueRightExpr);
                var comparisionExpr = Expression.Call(typeof(TestIncomingData_v1).GetMethod("Compare", BindingFlags.Public | BindingFlags.Static), new Expression[]
                {
                    Expression.Property(pe, valuePropInfo),
                    Expression.Constant(condition.Value)
                });
                var zero = Expression.Constant(0);
                var op = condition.Operator;
                Expression criterionExpression;
                switch (op)
                {
                    case "eq":
                        criterionExpression = Expression.Equal(comparisionExpr, zero);
                        break;
                    case "neq":
                        criterionExpression = Expression.NotEqual(comparisionExpr, zero);
                        break;
                    case "gt":
                        criterionExpression = Expression.GreaterThan(comparisionExpr, zero);
                        break;
                    case "gte":
                        criterionExpression = Expression.GreaterThanOrEqual(comparisionExpr, zero);
                        break;
                    case "lt":
                        criterionExpression = Expression.LessThan(comparisionExpr, zero);
                        break;
                    case "lte":
                        criterionExpression = Expression.LessThanOrEqual(comparisionExpr, zero);
                        break;
                    default:
                        throw new NotSupportedException($"Not supported operator: {op}");
                }
                if (criteriaExpression == null)
                {
                    criteriaExpression = Expression.And(nameExpression, criterionExpression);
                }
                else
                {
                    criteriaExpression = Expression.AndAlso(criteriaExpression, Expression.And(nameExpression, criterionExpression));
                }
            }
            if (criteriaExpression == null)
            {
                criteriaExpression = Expression.Constant(true);
            }
            var whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new[] { curType },
                queryableData.Expression,
                Expression.Lambda<Func<FieldDTO, bool>>(criteriaExpression, new[] { pe })
            );
            return whereCallExpression;
        }

        /*
        protected Expression ParseCriteriaExpression(
            IEnumerable<FilterConditionDTO> conditions,
            IQueryable<FieldDTO> queryableData)
        {
            var curType = typeof(FieldDTO);

            Expression criteriaExpression = null;
            var pe = Expression.Parameter(curType, "p");

            foreach (var condition in conditions)
            {
                var namePropInfo = curType.GetProperty("Key");
                var valuePropInfo = curType.GetProperty("Value");

                var nameLeftExpr = Expression.Property(pe, namePropInfo);
                var nameRightExpr = Expression.Constant(condition.Field);
                var nameExpression = Expression.Equal(nameLeftExpr, nameRightExpr);

                var valueLeftExpr = Expression.Invoke(TryMakeDecimalExpression.Value, Expression.Property(pe, valuePropInfo));
                var valueRightExpr = Expression.Invoke(TryMakeDecimalExpression.Value, Expression.Constant(condition.Value));
                var comparisionExpr = Expression.Call(valueLeftExpr, "CompareTo", null, valueRightExpr);
                var zero = Expression.Constant(0);

                var op = condition.Operator;
                Expression criterionExpression;

                switch (op)
                {
                    case "eq":
                        criterionExpression = Expression.Equal(comparisionExpr, zero);
                        break;
                    case "neq":
                        criterionExpression = Expression.NotEqual(comparisionExpr, zero);
                        break;
                    case "gt":
                        criterionExpression = Expression.GreaterThan(comparisionExpr, zero);
                        break;
                    case "gte":
                        criterionExpression = Expression.GreaterThanOrEqual(comparisionExpr, zero);
                        break;
                    case "lt":
                        criterionExpression = Expression.LessThan(comparisionExpr, zero);
                        break;
                    case "lte":
                        criterionExpression = Expression.LessThanOrEqual(comparisionExpr, zero);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("Not supported operator: {0}", op));
                }
                
                if (criteriaExpression == null)
                {
                    criteriaExpression = Expression.And(nameExpression, criterionExpression);
                }
                else
                {
                    criteriaExpression = Expression.AndAlso(criteriaExpression, Expression.And(nameExpression, criterionExpression));
                }
            }

            if (criteriaExpression == null)
            {
                criteriaExpression = Expression.Constant(true);
            }

            var whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new[] { curType },
                queryableData.Expression,
                Expression.Lambda<Func<FieldDTO, bool>>(criteriaExpression, new[] { pe })
            );

            return whereCallExpression;
        }*/

        private static readonly Lazy<Expression<Func<string, IComparable>>> TryMakeDecimalExpression =
            new Lazy<Expression<Func<string, IComparable>>>(() =>
            {
                var value = Expression.Parameter(typeof(string), "value");
                var returnValue = Expression.Variable(typeof(IComparable), "result");
                var decimalValue = Expression.Variable(typeof(decimal), "decimalResult");
                var ifExpression = Expression.IfThenElse(
                    Expression.Call(typeof(decimal), "TryParse", null,
                            Expression.TypeAs(value, typeof(string)), decimalValue),
                    Expression.Assign(returnValue, Expression.TypeAs(decimalValue, typeof(IComparable))),
                    Expression.Assign(returnValue, Expression.TypeAs(value, typeof(IComparable))));
                var func = Expression.Block(
                    new[] { returnValue, decimalValue },
                    ifExpression,
                    returnValue);
                return Expression.Lambda<Func<string, IComparable>>(func, "TryMakeDecimal", new[] { value });
            });

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authToken)
        {
            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authToken);
        }

        protected virtual Crate CreateControlsCrate()
        {
            var fieldFilterPane = new FilterPane()
            {
                Label = "Execute Actions If:",
                Name = "Selected_Filter",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Queryable Criteria",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                },
                // Events = new List<ControlEvent>() { ControlEvent.RequestConfig }
            };

            return PackControlsCrate(fieldFilterPane);
        }

        /// <summary>
        /// Looks for first Create with Id == "Standard Design-Time" among all upcoming Actions.
        /// </summary>
        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var curUpstreamFields =
                (await GetDesignTimeFields(curActivityDO, CrateDirection.Upstream, AvailabilityType.RunTime))
                .Fields
                .ToArray();

            //2) Pack the merged fields into a new crate that can be used to populate the dropdownlistbox
            // var queryFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Queryable Criteria", curUpstreamFields);
            var queryFieldsCrate = Crate.FromContent(
                "Queryable Criteria",
                new FieldDescriptionsCM(curUpstreamFields)
            );

            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var crateStorage = CrateManager.UpdateStorage(() => curActivityDO.CrateStorage))
            {
                crateStorage.Replace(AssembleCrateStorage(queryFieldsCrate, configurationControlsCrate));
            }

            return curActivityDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var curUpstreamFields =
                (await GetDesignTimeFields(curActivityDO, CrateDirection.Upstream, AvailabilityType.RunTime))
                .Fields
                .ToArray();

            //2) Pack the merged fields into a new crate that can be used to populate the dropdownlistbox
            // var queryFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Queryable Criteria", curUpstreamFields);
            var queryFieldsCrate = Crate.FromContent(
                "Queryable Criteria",
                new FieldDescriptionsCM(curUpstreamFields)
            );

            using (var crateStorage = CrateManager.UpdateStorage(() => curActivityDO.CrateStorage))
            {
                crateStorage.RemoveByLabel("Queryable Criteria");
                crateStorage.Add(queryFieldsCrate);
            }

            return curActivityDO;
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActionDataPackageDO)
        {
            if (CrateManager.IsStorageEmpty(curActionDataPackageDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var hasControlsCrate = GetCratesByManifestType<StandardConfigurationControlsCM>(curActionDataPackageDO) != null;

            var hasQueryFieldsCrate = GetCratesByManifestType<FieldDescriptionsCM>(curActionDataPackageDO) != null;

            if (hasControlsCrate && hasQueryFieldsCrate)
            {
                return ConfigurationRequestType.Followup;
            }
            else
            {
                return ConfigurationRequestType.Initial;
            }
        }

        protected async Task<Crate> ValidateFields(List<FieldValidationDTO> requiredFieldList)
        {
            var result = await HubCommunicator.ValidateFields(requiredFieldList, CurrentFr8UserId);

            var validationErrorList = new List<FieldDTO>();
            //lets create necessary validationError crates
            for (var i = 0; i < result.Count; i++)
            {
                var fieldCheckResult = result[i];
                if (fieldCheckResult == FieldValidationResult.NotExists)
                {
                    validationErrorList.Add(new FieldDTO() { Key = requiredFieldList[i].FieldName, Value = "Required" });
                }
            }

            if (validationErrorList.Any())
            {
                return CrateManager.CreateDesignTimeFieldsCrate("Validation Errors", validationErrorList.ToArray());
            }

            return null;
        }

        protected async Task<CrateDTO> ValidateByStandartDesignTimeFields(ActivityDO curActivityDO, FieldDescriptionsCM designTimeFields)
        {
            var fields = designTimeFields.Fields;
            var validationList = fields.Select(f => new FieldValidationDTO(curActivityDO.Id, f.Key)).ToList();
            return CrateManager.ToDto(await ValidateFields(validationList));
        }


        protected async Task<CrateDTO> ValidateActivity(ActivityDO curActivityDO)
        {
            return await ValidateByStandartDesignTimeFields(curActivityDO, CrateManager.GetStorage(curActivityDO).FirstCrate<FieldDescriptionsCM>(x => x.Label == "Queryable Criteria").Content);
        }

        /// <summary>
        ///  Returns true, if at least one row has been fully configured.
        /// </summary>
        /// <param name="curActionDataPackageDTO"></param>
        /// <returns></returns>
//        private bool HasValidConfiguration(ActionDTO curActionDataPackageDTO)
//        {
//            // STANDARD_CONF_CONTROLS_NANIFEST_NAME can't be deseralized to RadioButtonOption.

////            var crateDTO = GetCratesByManifestType(curActionDataPackageDTO, CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME);
////
////            if (crateDTO != null)
////            {
////                RadioButtonOption radioButtonOption = JsonConvert.DeserializeObject<RadioButtonOption>(crateDTO.Contents);
////                if (radioButtonOption != null)
////                {
////                    foreach (ControlDefinitionDTO controlDefinitionDTO in radioButtonOption.Controls)
////                    {
////                        if (!string.IsNullOrEmpty(controlDefinitionDTO.Value))
////                        {
////                            FilterDataDTO filterDataDTO = JsonConvert.DeserializeObject<FilterDataDTO>(controlDefinitionDTO.Value);
////                            return filterDataDTO.Conditions.Any(x =>
////                                  x.Field != null && x.Field != "" &&
////                                  x.Operator != null && x.Operator != "" &&
////                                  x.Value != null && x.Value != "");
////                        }
////                    }
////                }
////
////            }
//            return false;
//        }

        private Crate<TManifest> GetCratesByManifestType<TManifest>(ActivityDO curActionDataPackageDO)
        {
            string curLabel = string.Empty;

            if (typeof(TManifest) == typeof(FieldDescriptionsCM))
            {
                curLabel = "Queryable Criteria";
            } 
            else if (typeof(TManifest) == typeof(StandardConfigurationControlsCM))
            {
                curLabel = "Configuration_Controls";
            }

            return CrateManager.GetStorage(curActionDataPackageDO).FirstCrateOrDefault<TManifest>(x => x.Label == curLabel);
        }
    }
}
