# Design-Time Signalling of Crate and Field Data
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 

Signalling is a process that takes place in Design mode. The Terminal signals the types of data that an Activity will generate when executed in Run mode, so that the user can configure connections between Activities.

For example, suppose a User is building a Plan that will load customer information out of an Google Form and use that information to carry out a DocuSign mail merge so that each customer receives a customized DocuSign document and envelope.

When the Google terminal receives a configuration request for a Get Google Form Activity, it connects to Google, reads the field names of the Google form and puts them into a crate of Crate Description. It then adds this crate to the Activity’s crate storage. It then returns the Activity to the Hub. When the user goes to configure the DocuSign activity, the DocuSign terminal makes an “available upstream data” request to the Hub, which assembles all available Crate Descriptions and returns them in a crate to the Terminal. The Terminal then uses these to populate drop down list boxes that are part of mapping controls like TextSource. The user can then, still at Design Time, map fields from the Google form to fields in their chosen DocuSign Template.

This dynamic loading of upstream field information allows activities to remain completely decoupled while still enabling easy user mapping of data from one activity to the next.

Crate Description allows activity to signal about both fields and crates that will be available in Run mode. Each Crate Description can describe several crates that will be available. Fields and related manifests are tightly copuled: field can exist only wihtin manifest. So, for each signaled crate activity should provide list of available fields.

Here is an example of JSON representation of Crate Description:
```javascript
{
    “manifestType”: “Crate Description”,
    “manifestId”: 32,
    “manifestRegistrar”: “www.fr8.co/registry”,
    “id”: “b4eba516-b972-4152-a277-f647b9cc853c”,
    “label”: “Runtime Available Crates”,
    “contents”: {
    “CrateDescriptions”: [
      {
      “manifestId”: 5,
      “manifestType”: “Standard Payload Data”,
      “label”: “Build Message”,
      “producedBy”: “Build a Message”,
      “availability”: 2,
      “fields”: [
        {
          “key”: “Name”,
        },
        {
          “key”: “ObjectType”,
        },
        {
          “key”: “NotificationMessage”,
        }
      ]
      }
    ]
    },
    “createTime”: “”,
    “availability”: 2
}
```

Here is how we can read JSON above: In Run mode activity will place crate of manifest “Standard Payload Data”. This manifest will have three field with names: Name, ObjectType and NotificationMessage.

For each field activity can specify a vareity of different properties in addition to field’s name. The most important are:
* *label* - user friendly text to be displayed in the UI
* *fieldType* – type of this field. We use the same type names as Salesforce.

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md) 
