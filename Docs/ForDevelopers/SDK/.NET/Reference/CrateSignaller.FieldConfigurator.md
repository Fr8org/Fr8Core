# FieldConfigurator

Allows to configure list of available fields for a certain manifest during the process of available crate signaling. 

**Namespace**: Fr8.TerminalBase.Infrastructure.CrateSignaller  
**Assembly**: Fr8TerminalBase.NET


## Methods
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| AddFields(IEnumerable\<FieldDTO>) | Mark fields from the given list of **FieldDTO** as available. All information from passed **FielDTO**s is respected with exception to: **Availability**, **SourceCrateLabel**,**SourceCrateManifest** and **SourceActivityId** |
| AddField(FieldDTO) | Mark field as available. All information from passed **FielDTO** is respected with exception to: **Availability**, **SourceCrateLabel**,**SourceCrateManifest** and **SourceActivityId**. |
| AddFields(params string[]) | Mark fields with the given names as available. When using this method the fields which are being signaled will have the same label as name and no extended information (like type, or list of allowed values) will be passed. |
| AddField(string) | Mark field with the given name as available. When using this method the field which is being signaled will have the same label as name and no extended information (like type, or list of allowed values) will be passed. |
 
## Remarks
Each method of this class returns the same instance. This allows to chain method calls:
```C#
 CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RunTimeCrateLabel, true).AddField("field1").AddField("field2");
```