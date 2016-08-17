# DropDownList Control

## Example Control Payload
```json
{
    "type": "DropDownList",      
    "listItems": [
        {
            "selected": true,
            "key": "Item key 1",
            "value": "Item value 1"
        },
        {
            "selected": false,
            "key": "Item key 2",
            "value": "Item value 2"
        },
        {
            "selected": false,
            "key": "Item key 3",
            "value": "Item value 3"
        }
    ],
    "name": "target_docusign_template",
    "required": true,
    "value": null,
    "label": "target_docusign_template",
    "events": [
        {
            "name": "onSelect",
            "handler": "requestConfig"
        }
    ]
}
```

## Working with upstream values

DropDownList control can be simply configured to display values provided by upstream activities. In order to do this you should omit manually filling `listItems` property and instead configure `source` property, setting its subproperty `requestUpstream` to `true`. Below is the example of DropDownList with this property configured

```json
{
    "type": "DropDownList",
    "name": "upstream_value",
    "required": true,
    "value": null,
    "label": "Take this value",
    "events": [
        {
            "name": "onSelect",
            "handler": "requestConfig"
        }
    ],
    "source": {
        "requestUpstream" : true,
        "manifestType": "Standard Design-Time Fields"
    }
}
```

If client discovers that DropDownList is configured this way, then everytime a dropdown part is open all upstream activities will be scanned for presence of crate of `Standard Design-Time Fields` manifest type. If one is found then its contents are extracted and placed as DropDownList items. Selecting such item effectively creates dependency between the owner of the DropDownList and the owner of crate produced.

Currently upstream source supports only manifest of type `Standard Design-Time Fields` but in the future it will most likely support other manifest types. `requestUpstream` and `manifestType` are the two mandatory properties but this way you may see the fields that you don't expect (e.g. activity stores some configuration-specific fields inside its storage). In order to filter out unwanted fields you may use two more optional properties. `label` is a string value and is designed to reference crate with corresponding label whil `availability` is an integer and is designed to filter out fields that are supposed to show up only at design-time or run-time. Below are the possible values of this property:

- 0 - availability is not set. This is the default value;
- 1 - value is available at design-time. Most activities will use this to store configuration-related values;
- 2 - value is available at run-time. These are fields that activity expect to produce during its execution;
- 3 - value is available at design-time OR run-time. This value covers both above cases at the same time.

Taking into account this information we could rewrite the above JSON example in the following way:

```json
{
    "type": "DropDownList",
    "name": "upstream_value",
    "required": true,
    "value": null,
    "label": "Take this value",
    "events": [
        {
            "name": "onSelect",
            "handler": "requestConfig"
        }
    ],
    "source": {
        "requestUpstream" : true,
        "manifestType": "Standard Design-Time Fields",
        "label": "Available Fields",
        "availability" : 2
    }
}
```

Learn more about manifest types [here](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/CratesManifest.md) and about upstream activities [here](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/OperatingConcepts/Signaling.md)
