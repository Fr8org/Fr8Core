#RadioButtonOption Control

A RadioButtonOption control it is a part of RadioButtonGroup Control

##Example Control Payload
```json
{
          "selected": true,
          "name": "specific",
          "value": "This specific value",
          "controls": [
            {
              "listItems": [ ],
              "name": "Select Upstream Crate",
              "required": false,
              "value": null,
              "label": "",
              "type": "DropDownList",
              "selected": false,
              "source": {
                "manifestType": "Standard Design-Time Fields",
                "label": "Upstream Plugin-Provided Fields"
              }
            }
}