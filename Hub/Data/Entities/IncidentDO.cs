using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Fr8.Infrastructure.Utilities;

namespace Data.Entities
{
    public class IncidentDO : HistoryItemDO
    {
        public IncidentDO()
        {
            Priority = 1;
            //Notes = "No additional notes";
        }

        public int Priority { get; set; }
        

        [NotMapped]
        public bool IsHighPriority { get { return Priority >= 5; } }

        public override void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            base.OnModify(originalValues, currentValues);

            var reflectionHelper = new ReflectionHelper<IncidentDO>();
            var priorityPropertyName = reflectionHelper.GetPropertyName(i => i.Priority);
            if (!MiscUtils.AreEqual(originalValues[priorityPropertyName], currentValues[priorityPropertyName])
                && IsHighPriority)
            {
                EventManager.HighPriorityIncidentCreated(Id);
            }
        }

        public override void AfterCreate()
        {
            base.AfterCreate();

            if (IsHighPriority)
            {
                EventManager.HighPriorityIncidentCreated(Id);
            }
        }
    }
}
