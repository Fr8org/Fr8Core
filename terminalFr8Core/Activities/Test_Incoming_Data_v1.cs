using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Data.Infrastructure;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Newtonsoft.Json;

namespace terminalFr8Core.Activities
{
    public class Test_Incoming_Data_v1 : ExplicitTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Test_Incoming_Data",
            Label = "Test Incoming Data",
            Category = ActivityCategory.Processors,
            Version = "1",
            MinPaneWidth = 550,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                new ActivityCategoryDTO(TerminalData.WebServiceDTO.Name, TerminalData.WebServiceDTO.IconPath)
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public Test_Incoming_Data_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        protected List<KeyValueDTO> GetAllPayloadFields()
        {
            var valuesCrates = Payload.CrateContentsOfType<StandardPayloadDataCM>();
            var curValues = new List<KeyValueDTO>();
            foreach (var valuesCrate in valuesCrates)
            {
                curValues.AddRange(valuesCrate.AllValues());
            }
            return curValues;
        }

        private bool Evaluate(string criteria, Guid processId, IEnumerable<KeyValueDTO> values)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));
            if (criteria == string.Empty)
                throw new ArgumentException("criteria is empty", nameof(criteria));
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            return Filter(criteria, processId, values.AsQueryable()).Any();
        }

        private IQueryable<KeyValueDTO> Filter(string criteria,
            Guid processId, IQueryable<KeyValueDTO> values)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));
            if (criteria == string.Empty)
                throw new ArgumentException("criteria is empty", nameof(criteria));
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var filterDataDTO = JsonConvert.DeserializeObject<FilterDataDTO>(criteria);
            if (filterDataDTO.ExecutionType == FilterExecutionType.WithoutFilter)
            {
                return values;
            }
            EventManager.CriteriaEvaluationStarted(processId);
            return filterDataDTO.Conditions.Select(condition => ParseCriteriaExpression(condition, values)).Aggregate<Expression, IQueryable<KeyValueDTO>>(null, (current, filterExpression) => current?.Provider.CreateQuery<KeyValueDTO>(filterExpression) ?? values.Provider.CreateQuery<KeyValueDTO>(filterExpression));
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
        
        protected Expression ParseCriteriaExpression(FilterConditionDTO condition, IQueryable<KeyValueDTO> queryableData)
        {
            var curType = typeof(KeyValueDTO);
            Expression criteriaExpression = null;
            var pe = Expression.Parameter(curType, "p");

            var namePropInfo = curType.GetProperty("Key");
            var valuePropInfo = curType.GetProperty("Value");
            var nameLeftExpr = Expression.Property(pe, namePropInfo);
            var nameRightExpr = Expression.Constant(condition.Field);
            var nameExpression = Expression.Equal(nameLeftExpr, nameRightExpr);
            var comparisionExpr = Expression.Call(typeof(Test_Incoming_Data_v1).GetMethod("Compare", BindingFlags.Public | BindingFlags.Static), new Expression[]
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

            criteriaExpression = Expression.And(nameExpression, criterionExpression);

            var whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new[] { curType },
                queryableData.Expression,
                Expression.Lambda<Func<KeyValueDTO, bool>>(criteriaExpression, new[] { pe })
            );
            return whereCallExpression;
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
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                    RequestUpstream = true
                }
            };

            return PackControlsCrate(fieldFilterPane);
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
            catch (Exception)
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
            var configurationControlsCrate = CreateControlsCrate();
            Storage.Add(configurationControlsCrate);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}
