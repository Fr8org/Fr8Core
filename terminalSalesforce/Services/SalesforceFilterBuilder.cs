using System;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Logging;
using terminalSalesforce.Infrastructure;

namespace terminalSalesforce.Services
{
    public class SalesforceFilterBuilder : ISalesforceFilterBuilder
    {
        public string BuildFilter(IList<FieldDTO> fields, ICollection<FilterConditionDTO> conditions)
        {
            if (conditions == null || conditions.Count == 0 || fields == null || fields.Count == 0)
            {
                return string.Empty;
            }
            var fieldByName = new SafeDictionary<string, FieldDTO>(fields.ToDictionary(x => x.Name));
            var result = new List<string>(conditions.Count);
            foreach (var condition in conditions)
            {
                var field = fieldByName[condition.Field];
                if (field == null)
                {
                    Logger.GetCurrentClassLogger().Warn($"Field {condition.Field} exists in condition but not in the list of fields");
                    continue;
                }
                var value = NormalizeValue(field, condition.Value);
                var operation = NormalizeOperator(field, condition.Operator);
                result.Add($"{condition.Field}{operation}{value}");
            }
            return string.Join(" AND ", result);
        }

        private string NormalizeOperator(FieldDTO field, string @operator)
        {
            //Some field type may require special operators treatment. Thus field is passed here too
            switch (@operator)
            {
                case "eq":
                case "=":
                case "==":
                    return "=";
                case "neq":
                case "!=":
                    return "!=";
                case ">":
                case "gt":
                    return ">";
                case ">=":
                case "gte":
                    return ">= ";
                case "<":
                case "lt":
                    return "<";
                case "<=":
                case "lte":
                    return "<=";
                default:
                    throw new NotSupportedException($"Not supported operator: {@operator}");
            }
        }

        private string NormalizeValue(FieldDTO field, string value)
        {
            switch (field.FieldType)
            {
                case FieldType.Date:
                    return DateTime.ParseExact(value, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                case FieldType.DateTime:
                    return DateTime.ParseExact(value, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-ddTHH:mm:ssZ");
                case FieldType.Currency:
                case FieldType.Double:
                    return value;
                default:
                    return $"'{value}'";
            }
        }
    }
}