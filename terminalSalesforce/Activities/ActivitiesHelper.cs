using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using System.Collections.Generic;
using System.Linq;

namespace terminalSalesforce.Actions
{
    public static class ActivitiesHelper
    {
        public static void FillSalesforceSupportedObjects(Crate configurationCrate, string controlName)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var control = configurationControl.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                control.ListItems = GetAvailableFields();
            }
        }

        private static List<ListItem> GetAvailableFields()
        {
            var fields =
            new FieldDTO[]
                {
                    new FieldDTO("Account") { Availability = AvailabilityType.Configuration},
                    new FieldDTO("Contact") { Availability = AvailabilityType.Configuration},
                    new FieldDTO("Lead") { Availability = AvailabilityType.Configuration},
                    new FieldDTO("Opportunity") { Availability = AvailabilityType.Configuration},
                    //new FieldDTO("Forecast") {Availability = AvailabilityType.Configuration},
                    new FieldDTO("Contract") { Availability = AvailabilityType.Configuration},
                    new FieldDTO("Order") { Availability = AvailabilityType.Configuration},
                    new FieldDTO("Case") { Availability = AvailabilityType.Configuration},
                    new FieldDTO("Solution") { Availability = AvailabilityType.Configuration},
                    new FieldDTO("Product2") { Availability = AvailabilityType.Configuration},
                    new FieldDTO("Document") { Availability = AvailabilityType.Configuration}
                    //new FieldDTO("File") {Availability = AvailabilityType.Configuration}
                };
            return fields.Select(x => new ListItem() { Key = x.Key, Value = x.Key }).ToList();
        }
    }
}