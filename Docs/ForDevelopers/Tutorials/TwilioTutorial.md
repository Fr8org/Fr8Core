# TUTORIALS: A BASIC TWILIO TERMINAL THAT CAN SEND AN SMS MESSAGE

Suppose you want to make a Send an SMS Message available to Fr8 Users.  This first example will be ultra-simple. IWe’ll assume that you already have build a Terminal that communicates with Twilio via Twilio’s RESTful APIs, and that has appropriate developer keys and tokens to be allowed to make calls (Learn more about Terminal Development and Authentication)

## Step 1: Define the ActivityTemplate for your Action and make it discoverable by Hubs

Hubs that know about your Terminal will contact it when they Startup and periodically thereafter and request an enumeration of the available Actions. If you’re adding an Action to an existing Terminal, you’ll want to modify the code that responds to /terminals/discover

## Step 2: Define the Configuration Controls UI for your Action

When a Hub has discovered your Action, it will make it available to Users who are assembling Routes. When a User selects your action, the client will POST to its Hub at /actions/configure, and the Hub will pass that call along to your Terminal. In both cases the Action JSON is passed along as a parameter. At this point, the Terminal’s job is to assemble instructions that the client can use to show any desired Configuration UI.

Actions craft their UI by assembling a set of JSON composed from Fr8’s predefined UI controls. These Control Definitions are packed into a Crate with a “Standard UI Controls” Manifest, and the Crate is added to the Action, so that when the Action is serialized and returned from the Terminal in JSON form, the Crate of Standard UI Controls will be included in that JSON.

From the Twilio API you learn (to no great surprise) that you will need a telephone number and a message text in order to send an SMS. The first important task is to pack the UI Controls Crate with two textFields, one for the phone number and one for the message body.

The client will be responsible for rendering the text fields, and saving any user inputs back into the same UI Controls Crate

## Step 3: Write the Run-Time Functionality

When a Route containing your Action is executed, the Hub doing the coordination will call your Terminal’s /run endpoint and pass it the Container that it has been shepherding from Action to Action.

In this case, your code will simply look into the UI Controls Crate associated with this Action, extract the two values that the User provided, and then make the appropriate RESTful call to Twilio.

After making that call you can simply return a response to the Hub.
