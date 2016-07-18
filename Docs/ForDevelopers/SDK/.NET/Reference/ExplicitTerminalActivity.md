# ExplicitTerminalActivity

Base class for activities that requires highly dynamic UI that can't be implemented using **TerminalActivity\<TUi>**. This is also a base class for legacy Fr8 activities.

**Namespace**: Fr8.TerminalBase.BaseClasses  
**Assembly**: Fr8TerminalBase.NET

## Inheritance Hierarchy
* IActivty
  * TerminalActivityBase
    * **ExplicitTerminalActivity**


## Properties
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| ConfigurationControls | Direct access to current activity StandardConfigurationControlsCM crate. All changes made to controls inside this crate will be persisted if these changes are done during processing configuration requests. All changes made to UI during activity execution will be discarded. **ConfigurationControlsCM** crate is automatically created on the first request to **ConfigurationControls** property in case if current request is not related to activity execution. In case of activity execution **ConfigurationControls** will return **null** |


## Methods
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| GetControl\<T>(string) | Search for control with the given name in **ConfigurationControlsCM** crate. Returns **null** if no control is found. |
| RemoveControl\<T>(string) | Remove control with the given name from  **ConfigurationControlsCM** crate |
| AddLabelControl(string, string, string) | Adds new TextBlock control to the UI |
| AddControl(ControlDefinitionDTO) | Add control to the Ui |
| AddControls(IEnumerable<ControlDefinitionDTO>) | Add collection of controls to the Ui |
| AddControls(params ControlDefinitionDTO[]) | Add collection of controls to the Ui |

## Remarks

This base class should be used when you need the most possible flexibility while working with activity UI. **ExplicitTerminalActivity** allows to add and remove UI controls at any time, that is impossible with **TerminalActivity\<TUi>**. So if your activity requires a highly dynamic UI that can radically changes during user interaction use this class as the base. But in any cases check the possibility of using  **TerminalActivity\<TUi>** first, because it allows to reduce amount of code that works with UI and helps to improve reliability.