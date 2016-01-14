using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    public class TrackingStatusDO : BaseObject, ICustomField
    {
        public int Id { get; set; }
        public string ForeignTableName { get; set; }

        [ForeignKey("TrackingTypeTemplate")]
        public int? TrackingType { get; set; }
        public _TrackingTypeTemplate TrackingTypeTemplate { get; set; }

        [ForeignKey("TrackingStatusTemplate")]
        public int? TrackingStatus { get; set; }
        public _TrackingStatusTemplate TrackingStatusTemplate { get; set; }
    }
}
