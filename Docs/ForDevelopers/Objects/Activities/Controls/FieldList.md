#FieldList Control

<img src="images/FieldList.PNG" width="200" alt="FieldList UI"/>

##Example Control Payload
```javascript
{
      "type": "FieldList",      
      "name": "Selected_Fields",
      "required": true,
      "selected": false,
      "value": [
          {"Key":"",
           "Value":""
          }
       ],
      "label": "Fill the values for other actions",
      "events": [
        {
          "name": "onSelect",
          "handler": "requestConfig"
        }
      ]
}
```
