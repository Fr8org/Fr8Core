# CrateSignaller

Service for signalling about the crates that should be seen by other activities. 

**Namespace**: Fr8.TerminalBase.Infrastructure  
**Assembly**: Fr8TerminalBase.NET


## Methods
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| ClearAvailableCrates ()| Remove all information about available crates. |
| MarkAvailableAtRuntime\<TManifest> (string, bool) | Signal that crate with the given manifest and label can be found in the Conatiner's Payload after activity finishes execution |
| MarkAvailableAtDesignTime\<TManifest> (string, bool) | Signal that crate with the given manifest and label can be found in the current activity's storage during the design-time |
| MarkAvailableAlways\<TManifest> (string, bool) | Signal that crate with the given manifest and label can be found in the current activity's storage during the design-time and in the container's payload after activity finishes execution. |
| MarkAvailable\<TManifest> (string, AvailabilityType, bool) | Signal about the selected availability of crate with given manifest and label.
| MarkAvailable (CrateManifestType, string, AvailabilityType, bool) | Signal about the selected availability of crate with given manifest and label.

## Remarks
By default each **MarkAvailable** method family signals about all public fields and properties of the manifest. This means that other activities that needs access to the individual fields will be able to "see" them. You can control this behavior by using **suppressFieldDiscovery** parameter. If you set it to **true** than no fields or properties will be signaled. Another way to control what fields are signaled is to use attribute [ManifestFieldAttribute](/Docs/ForDevelopers/SDK/.NET/Reference/ManifestFieldAttribute.md).

You can manually specify fields of your manifest. In case of **StandardPayloadDataCM** and **StandardTableDataCM** you always have to do this, because this manifests can contain any data and the fields of this data can not be deduced from the manifest itself. To manually specify fields you should use the return value of **MakeAvailable** method family. All these methods return an instance of [FieldConfigurator](/Docs/ForDevelopers/SDK/.NET/Reference/CrateSignaller.FieldConfigurator.md) that is intended to control what fields will available for the certain manifest.

 
## Example of usage

Suppose you are writing activity that will report weather. You have decided that it will report the weather in the form of **StandardPayloadDataCM** with the following fields:  
1. Temperature
2. Humidity
3. Wind


So you need to signal about **StandardPayloadDataCM** during activity's initial configuration:
```C#
public override async Task Initialize()
{
	CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>("CurrentWeather").AddFields("Temperature", "Humidity", "Wind");
}
```
> **Note**: Always use clear self-describing labels for crates. Do not use labels like: 'RunTimeData' or 'StandardPayloadDataCM', 'Payload', 'Data', etc.

During activity execution you now have to put **StandardPayloadDataCM** crate with label 'CurrentWeather':

```C#
public override async Task Run()
{
	Payload.Add("CurrentWeather", new StandardPayloadDataCM(
				new KeyValueDTO("Temperature", "24C"), 
				new KeyValueDTO("Humidity", "72%"),
				new KeyValueDTO("Wind", "6 km/h W")));
}
```