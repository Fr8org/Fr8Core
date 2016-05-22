# Terminal Activities
Terminal Activities, or simply Activities, are integral parts of the Fr8 Terminal architecture. Activities define the specific functionality available to a Terminal as well as the UI controls that are available for the activity.

## Security

## Base Implementation

## Enhanced Terminal
Should developers be encouraged to extend the EnhancedTerminalActivity instead of BaseTerminalActivity?

## Fields
* CrateManager
* CurrentFr8UserId
* CurrentFr8UserEmail
* ActivityName
* ActivityTemplateCache
* IHubCommunicator
* ExcludedManifestTypes

## Constructors
* BaseTerminalActivity()

  The no parameter constructor calls the single parameter constructor with the value "unknown". This constructor shold not be used.
* BaseTerminalActivity(string name)

  The single parameter constructor creates a new BaseTerminalActivity instance and sets the ActivityName field to the value of the name parameter. This constructor also initializes the CrateManager and HubCommunicator fields

## Methods
* SetCurrentUser

