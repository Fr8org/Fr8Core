#DropDownList Control

##Example Control Payload
```javascript
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
      ],
      "source": {
        "manifestType": "Standard Design-Time Fields",
        "label": "Available Templates"
      }
}
```
