#ControlDefinitionDTO

The ControlDefinitionDTO is the super class for all the Control objects. The fields that it defines are used by the Hub to correctly render the

##Fields

Field |	Is required? |	 Details
--- | --- | ---
type |	yes |	one of the [ControlTypes](../Activities/Controls.md#control-types)
name |	yes |	should be unique among the controls added to a single Crate.
label |	no |	intended for friendly ui-visible labels. If not present, the client will use the name field
required |	yes |	true or false. if set to true, the client or server will validate that a value has been set before allowing the field
events |	no |	List of [ControlEvent](../Activities/Controls/ControlEvent.md) objects which allow the control to request that action take place upon specific JavaScript events . Discussed [here](https://maginot.atlassian.net/wiki/display/SH/Supported+Configuration+Control+Events).
source |	no |	instructs some controls where to find data.
showDocumentation |	no |	 used to provide documentation for activities
selected | no | specifies whether the control has been selected
isHidden | no | specifies whether the control should be hidden
isCollapsed | no | specifies whether the control is collapsed
errorMessage |	no |	![ErrorMessage](https://github.com/Fr8org/Fr8Core/blob/master/Docs/img/ErrorMessage.png) used to inform a user that an error has occurred
