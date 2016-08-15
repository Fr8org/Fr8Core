#FilterPane Control

A widget that produces UI which generates a query. Looks like this: ![FilterPane](/Docs/img/FilterPane.png)

The customer can add up to 10 of the criteria. All but the first one should have a small delete "X" button in the upper right.

The operators are:
- greater than
- greater than or equal
- less than
- less than or equal
- equal
- not equal

For V1, the value field is a simple editable text field (not the fancier widget shown above. Also there's no need for the red asterixes)
The values for Field are text values that will be provided by the server in a list

The client needs to package up the values of these base units upon Submit and pass data to the server. Use the mechanisms implemented as part of DO-763

##Technical Limitation of the Filter Pane control:
As per the current implementation, the Filter Pane uses a Design Time Fields crate to populate the left side Drop Down List Boxes. This crate should be labelled as Queryable Criteria to make this control to consider your design time fields crate.

##Example Control Payload
```json
{
        "resolvedUpstreamFields": [],
        "fields": null,
        "name": "Selected_Filter",
        "required": true,
        "value": "{\"executionType\":\"1\",\"conditions\":[{\"field\":\"a\",\"operator\":\"gt\",\"value\":\"0\"},{\"field\":\"b\",\"operator\":\"eq\",\"value\":\"2\"},{\"field\":\"c\",\"operator\":\"lte\",\"value\":\"4\"}]}",
        "label": "Execute Actions If:",
        "type": "FilterPane",
        "selected": false,
        "events": [],
        "source": {
        "manifestType": "Field Description",
        "label": "Upstream Terminal-Provided Fields",
        "filterByTag": null,
        "requestUpstream": true,
        "availabilityType": 0
        },
        "showDocumentation": null,
        "isHidden": false,
        "isCollapsed": false,
        "errorMessage": null
}
```
