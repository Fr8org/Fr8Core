using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Activities
{
    public class TestIncomingData_v1 : BaseTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "TestIncomingData",
            Label = "Test Incoming Data",
            Category = ActivityCategory.Processors,
            Version = "1",
            MinPaneWidth = 550,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public TestIncomingData_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        protected List<FieldDTO> GetAllPayloadFields()
        {
            var valuesCrates = Payload.CrateContentsOfType<StandardPayloadDataCM>();
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
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                    RequestUpstream = true
                },
                // Events = new List<ControlEvent>() { ControlEvent.RequestConfig }
            };

            return PackControlsCrate(fieldFilterPane);
        }

        protected async Task<Crate> ValidateFields(List<FieldValidationDTO> requiredFieldList)
        {
            var result = await HubCommunicator.ValidateFields(requiredFieldList);
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

        protected async Task<CrateDTO> ValidateByStandartDesignTimeFields(FieldDescriptionsCM designTimeFields)
        {
            var fields = designTimeFields.Fields;
            var validationList = fields.Select(f => new FieldValidationDTO(ActivityId, f.Key)).ToList();
            return CrateManager.ToDto(await ValidateFields(validationList));
        }

        protected async Task<CrateDTO> ValidateActivity()
        {
            return await ValidateByStandartDesignTimeFields(Storage.FirstCrate<FieldDescriptionsCM>(x => x.Label == "Queryable Criteria").Content);
        }


        public override async Task Run()
        {
            await RunTests();
        }

        public virtual async Task RunTests()
        {
            var filterPaneControl = GetControl<FilterPane>("Selected_Filter");
            if (filterPaneControl == null)
            {
                RaiseError("No control found with Type == \"filterPane\"");
            }
            var curValues = GetAllPayloadFields();
            // Prepare envelope data.
            // Evaluate criteria using Contents json body of found Crate.
            bool result = false;
            try
            {
                result = Evaluate(filterPaneControl.Value, ExecutionContext.ContainerId, curValues);
            }
            catch (Exception e)
            {
            }

            if (!result)
            {
                TerminateHubExecution();
                return;
            }

            Success();
        }

        public override async Task Initialize()
        {
           /* var curUpstreamFields = (await GetDesignTimeFields(CrateDirection.Upstream, AvailabilityType.RunTime))
                .Fields
                .ToArray();
            //2) Pack the merged fields into a new crate that can be used to populate the dropdownlistbox
            // var queryFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Queryable Criteria", curUpstreamFields);
            var queryFieldsCrate = Crate.FromContent(
                "Queryable Criteria",
                new FieldDescriptionsCM(curUpstreamFields)
            );
            */
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();
            Storage.Add(configurationControlsCrate);
           // Storage.Add(queryFieldsCrate);
        }

        public override async Task FollowUp()
        {
           /* var curUpstreamFields = (await GetDesignTimeFields(CrateDirection.Upstream, AvailabilityType.RunTime))
                .Fields
                .ToArray();
            //2) Pack the merged fields into a new crate that can be used to populate the dropdownlistbox
            // var queryFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Queryable Criteria", curUpstreamFields);
            var queryFieldsCrate = Crate.FromContent(
                "Queryable Criteria",
                new FieldDescriptionsCM(curUpstreamFields)
            );
            Storage.RemoveByLabel("Queryable Criteria");
            Storage.Add(queryFieldsCrate);*/
        }
    }
}
