# FR8 MODES

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
 
 Fr8 has 3 Modes:

  Mode | Description   
  --- | ----   
 Design Mode | Design Creation Mode allows Fr8 users to create and modify Fr8 Plans and Activities. This is the domain of the Power User, with full access to the Plan Builder.   
  Data Entry Mode | In Data Entry Mode, users can configure Activities but not modify Plans.  Power Users can configure Plans to be executed by unsophisticated users in Data Entry Mode, and those users will not see complex menus.   
 [Activation Mode](https://github.com/Fr8org/Fr8Core/blob/master/ForDevelopers/OperatingConcepts/PlanActivationAndRunning.md) | When a Plan is first constructed and then Run, the Hub will seek to Activate it.   
 [Run Mode](https://github.com/Fr8org/Fr8Core/blob/master/ForDevelopers/OperatingConcepts/PlanActivationAndRunning.md) | Run Mode is non-interactive.   
 
### Design Mode

The core of Design Modeis the configuration process, which allows a Fr8 client to display to the user custom UI that’s specific to each Fr8 Activity. This provides a consistent user experience and frees Terminal and Activity builders from having to deal with front-end issues while giving them the opportunity collect any necessary configuration data from the user.

The Fr8 client does not have any specific knowledge about a specific Activity. Instead, it knows how to render a set of configuration controls, ranging from simple standards like checkboxes and text fields to more elaborate controls like query widgets.

The heart of Design Mode is [Activity Configuration.](https://github.com/Fr8org/Fr8Core/blob/master/ForDevelopers/OperatingConcepts/ActivityConfiguration.md)

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
