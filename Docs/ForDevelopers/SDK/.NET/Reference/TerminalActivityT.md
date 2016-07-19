# TerminalActivity\<TUi>

Recommended base class for developing new activities.

**Namespace**: Fr8.TerminalBase.BaseClasses  
**Assembly**: Fr8TerminalBase.NET

## Inheritance Hierarchy
* IActivty
  * TerminalActivityBase
    * **TerminalActivity\<TUi>**


## Properties
| Name                            |Description                                                                                 |
|---------------------------------|------------------------------------------------------------------------------------------- |
| ActivityUI | Representation of current activity StandardConfigurationControlsCM crate in the form of the class defining the UI. All changes made to controls in ActivityUI will be persisted if these changes are done during processing configuration requests. All changes made to UI during activity execution will be discarded. |

This class doesn't have specific methods to override or to use.

## Remarks
**TerminalActivity\<TUi>** allows you to define activity UI in declarative manner. Here is how activity UI can be defined:
```C#
public class ActivityUi : StandardConfigurationControlsCM
{
	public DropDownList ChannelSelector { get; set; }

	public TextSource MessageSource { get; set; }

	public ActivityUi(UiBuilder uiBuilder)
	{
		ChannelSelector = new DropDownList {
			Label = "Select Slack Channel",
			Events = new List<ControlEvent> { ControlEvent.RequestConfig }
		};
		
		MessageSource = uiBuilder.CreateSpecificOrUpstreamValueChooser("Message", nameof(MessageSource), addRequestConfigEvent: true);
		Controls.Add(ChannelSelector);
		Controls.Add(MessageSource);
	}
}
```

When defining an Activity, you add an ActivityUI class, which is an interface to a configuration crate (StandardConfigurationCotnrolsCM). You should use this ActivityUI  class as the template parameter of **TerminalActivity\<TUi>** than.  Main idea here is assign instances of all controls you are going to use during run-time or configuration to ActivityUi  properties. This (assignment) can be done using anyway you like including the way shown above. But it is a mandatory:

	You do all your manipulation of configuration controls via the ActivityUI

Generally, you don't have to worry about how this all would work. The fact is when you configuration or run-time code are working you already have valid instance of  ActivityUi in ConfigurationControls property filled with default values or with values that were sent from the client. And when you make changes to ConfigurationControls  all changes will be persisted in current activity's crate storage and be sent to the client. You don't have to check ActivityUi  properties for null. Once initialized in ActivityUi constructor all properties would point to a valid object instance.

Here is an example of how you can work with ActivityUI:

```C#
public override async Task Initialize()
{
	ActivityUI.ChannelSelector.ListItems = new [] 
	{
		new ListItem ({ Key = "key1", Value = "value 1"}),
		new ListItem ({ Key = "key2", Value = "value 2"})
	};
}

```
```C#
public override async Task Run()
{
	var channel = ActivityUI.ChannelSelector.Value;
    var message = ActivityUI.MessageSource.GetValue(Payload);
}
```


### Controlling visibility
If you need to hide/show controls in you UI your can take advantage of new property **IsHidden** of ControlDefinitionDTO.


### Dynamic controls support
Sometimes you need to have a collection of controls inside your activity UI. For example, you need to display some fields from the user select form. Obviously, that at the time of activity development you don't know what form user will select, so the number of controls and their configuration is unknown. Such collection of controls is called **dynamic controls**. Here is how you can define such collection (optionally together with regular controls):
```C#
pubic class ActivityUi
{
    public TextBlock Header;
     
    [DynamicControls]
    public readonly List<TextSource> TextSources = new List<TextSource>();
     
    public ActivityUi ()
    {
        Controls.Add (Header = new TextBlock () { Name = "Here are our dynamic controls:" });
    }
}
```

How this will work:

1. When you add control to the collection marked with DynamicControls attribute some prefix (name of the corresponding property)  will be automatically prepended to this control's name before this control will be stored into configuration controls crate.
2. When CC crate is being synced with ActivityUi, controls with names staring with specific substring will be put into the corresponding collection in ActivityUi. Prefix will be removed from control's name.

### How synchronization works

When CC crate is being synced with ActivityUI sequence of operations is the following:
1. New instance of ActivityUI is created.
2. All controls defined in ActivityUI are recursively traversed. If object that is derived from **IContainerControl** is encountered then objects returned from **EnumerateChildren()** method with be iterated over. 
3. The same algorithm as described in step 2 is used to recursively looks for all controls defined in Configuration Controls crate.
4. Controls from ActivityUI and Configuration Controls crate are matched using their names.
5. For each pair of matched control all public properties and fields of the control are iterated, and for each property the following conditions are checked:
   1. If member is marked with **IgnorePropertySyncAttribute** then member will be skipped
   2. If member is marked with **ForcePropertySyncAttribute** then synchronization will happen even if the conditions below are not true.
   3. If member has the type that is derived from  **IControlDefinition** then synchronization will be skipped
   4. If member has type of **IList<T>** where **T** is derived from **IControlDefinition** then synchronization will be skipped
6. If synchronization happens it works in the following way:
   1. If member can be written (member is a field or a property with setter) member value will be replaced.
   2. If member has no setter but it is a collection derived from **IList**, then the content of the collection will be replaced.