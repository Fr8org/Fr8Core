using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.TerminalBase.Helpers
{
    public static class FilterConditionHelper
    {
        public static string ParseConditionToText(List<FilterConditionDTO> filterData)
        {
            var parsedConditions = new List<string>();

            filterData?.ForEach(condition =>
            {
                string parsedCondition = condition.Field;

                switch (condition.Operator)
                {
                    case "eq":
                        parsedCondition += " = ";
                        break;
                    case "neq":
                        parsedCondition += " != ";
                        break;
                    case "gt":
                        parsedCondition += " > ";
                        break;
                    case "gte":
                        parsedCondition += " >= ";
                        break;
                    case "lt":
                        parsedCondition += " < ";
                        break;
                    case "lte":
                        parsedCondition += " <= ";
                        break;
                    default:
                        throw new NotSupportedException(string.Format("Not supported operator: {0}", condition.Operator));
                }

                parsedCondition += string.Format("'{0}'", condition.Value);
                parsedConditions.Add(parsedCondition);
            });

            var finalCondition = string.Join(" AND ", parsedConditions);

            return finalCondition;
        }
    }
}
