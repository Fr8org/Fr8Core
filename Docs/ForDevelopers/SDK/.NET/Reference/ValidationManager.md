# ValidationManager

Service for reporting activity configuration validation errors.

**Namespace**: Fr8.TerminalBase.Infrastructure  
**Assembly**: Fr8TerminalBase.NET



## Properties
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| HasErrors | Gets if current instance has any errors reported |
| ValidationResults | Gets reported errors as **ValidationResultsCM**. |
| Payload | Gets current container payload if validation manager is used during activity execution or **null** otherwise. |


## Methods
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| Reset() | Clears all validation errors |
| SetError (string, params ControlDefinitionDTO[]) | Report error that is binded to one or more controls |
| SetError (string, params string[]) | Report error that is binded to one or more control names |
| SetError (string) | Report activity wide error that is not binded to any specific control. |

## Extension methods
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| ValidateEmail (IConfigRepository, ControlDefinitionDTO, string) | Checks if control's value is a valid e-mail |
| ValidateEmail (IConfigRepository, TextSource, string) | Checks if TextSource's value is a valid e-mail. This method works both during the design-time and run-time. It can handle cases when TextSource is configured to extract value from the upstream. |
| ValidatePhoneNumber(string, TextSource) | Checks if the value is a valid phone number and bind validation error to the specific TextSource. |
| ValidateTextSourceNotEmpty(TextSource, string) | Checks if TextSource has a value. This method works both during the design-time and run-time. It can handle cases when TextSource is configured to extract value from the upstream.|
| ValidateCrateChooserNotEmpty(CrateChooser, string) | Checks if CrateChooser has a selected crate. This method works both during the design-time and run-time. It can handle cases when TextSource is configured to extract value from the upstream.|
| ValidateDropDownListNotEmpty(DropDownList, string) | Checks if DropDownListhas a selected item.|

## Remarks

**ValidationManager** allows you to write checks that can work both during the design-time and run-time. The key point here is **Payload** property. If it is not null you can consider that validation is running during activity execution and use **Payload** crate storage to extract values from the container accordingly corresponding UI controls settings.