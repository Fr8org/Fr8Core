#CrateDTO
The Crate Data Transfer Object contains data that is defined by the ManifestType field

##Fields
_string ManifestType_: The name of the manifest type which defines the data in the crate. Identified in JSON with the key "manifestType" and defined with getter and setter methods.

 _int ManifestId_: The ID of the manifest type. Identified in JSON with the key "manifestId" and defined with getter and setter methods.

_ManufacturerDTO Manufacturer_: The [ManufacturerDTO](ManufacturerDTO.md) object associated with the crate. Identified in JSON with the key "manufacturer" and defined with getter and setter methods.

_string ManifestRegistrar_: Returns the Fr8 URL for manifest registries: [http://www.fr8.co/registry](http://www.fr8.co/registry). Identified in JSON with the key "manifestRegistrar".

_string Id_: The ID for the crate, should be unique. Identified in JSON with the key "id" and defined with getter and setter methods.

_string Label_: The human-friendly label for the crate: Identified in JSON with the key "label" and defined with getter and setter methods.

_JToken Contents_: A JSON object which contains the data in the crate. Identified in JSON with the key "contents" and defined with getter and setter methods.

_string ParentCrateId_: The ID of the crate which contains this crate, if there is one. Identified in JSON with the key "parentCrateId" and defined with getter and setter methods.

_DateTime CreateTime_: The time at which this crate was created. Identified in JSON with the key "createTime" and defined with getter and setter methods. Serialized to JSON using the [CreateTimeConverter](Converters/CreateTimeConverter.md)

_AvailabilityType Availability_: The [AvailabilityType](../States/AvailabilityType.md) enum which identifies when the crate data is available. One of: NotSet, Configuration, RunTime, Always. Identified in JSON with the key "availability" and defined with getter and setter methods. Serialized to JSON using the [AvailabilityConverter](Converters/AvailablilityConverter.md).