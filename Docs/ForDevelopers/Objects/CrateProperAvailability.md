# CRATE PROPERTY: AVAILABILITY

[Go to Contents](https://github.com/Fr8org/Fr8Core.NET/blob/master/README.md)  

**Availability**

This property is currently used to filter out inappropriate choices from drop-down list boxes.

Allowable values are Run-Time, Configuration, and Both

The basic idea is this: if an Activity creates a Crate of design-time fields for use in a dropdown list box, those fields aren’t wanted in other drop down list boxes that dynamically fill with incoming run-time fields.

To filter out design-time fields, an Action should just add Avaiability=Run-Time to the query.

This is related to the Availabililty property on Fields.

[Go to Contents](https://github.com/Fr8org/Fr8Core.NET/blob/master/README.md)  
