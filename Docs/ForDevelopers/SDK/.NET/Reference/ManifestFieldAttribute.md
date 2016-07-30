# ManifestFieldAttribute

Allows to specify aspects of how the certain manifest field is being signaled when default setting for available crates signalling are used. 

**Namespace**: Fr8.Infrastructure.Data.Manifests  
**Assembly**: Fr8Infrastructure.NET


## Properties
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| Label | User friendly name that will be displayed to the user in the UI |
| IsHidden | Flag indicating wether this field should be automatically published in available fields when corresponding manifest is published as available | 



## Constructors
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| ManifestFieldAttribute () | Default constructor. **Label** is not set, **IsHidden** = **false** |
| ManifestFieldAttribute (string, bool) | Create an attribute with specified values of Label and IsHidden properties |

## Remarks

Example of usage:

``` C#
public class : Manifest
{
    [ManifestField(IsHidden = true)]
    public string Name { get; set; } 

    public List<PayloadObjectDTO> PayloadObjects { get; set; }

    [ManifestField(IsHidden = true)]
    public string ObjectType { get; set; }
}
``` 

If you signal about **StandardPayloadDataCM** then **Name** and **ObjectType** properties will never be marked as available for this manifest until you manually report about them using [FieldConfigurator](/Docs/ForDevelopers/SDK/.NET/Reference/CrateSignaller.FieldConfigurator.md).