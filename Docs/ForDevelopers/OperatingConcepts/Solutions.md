# Solutions

Fr8 Solutions are a set of preconfigured activities. It helps users to create preconfigured complex plans. A solution should know structures of it's child activities so that it can configure them.

## Step 1 - Solution Configuration

Let's assume you want to load data from Activity A (AA), process that data using Activity B (AB) and output that data to Activity C (AC). A newbie user might have hard time putting all those activities in a meaningful order with correct configuration. Solutions get in scene at this point. Let's call our solution Solution ABC. Solution ABC's UI will have necessary fields to gather information about AA, AB and AC. When user starts to fill required information Solution's configuration gets triggered. On this stage solution checks if all required configuration is ready.

## Step 2 - Child Activity Creation and Configuration

If user filled necessary information, solution loads all activity templates from HUB. Finds ActivityTemplate of A. Requests HUB to configure it. After HUB's response we have complete structure of AA. Assuming we know how AA works - we find UI crate of AA. Modify it's controls according to our target and according to user inputs on our solution. We do same operation for AB and AC. After modifying all Activities we add those activities as our children.

We return our ActivityDTO to HUB and voila - user sees Solution ABC with AA,AB and AC as children and preconfigured for our target.

# Flow

![alt tag](https://raw.githubusercontent.com/Fr8org/Fr8Core/master/Docs/ForDevelopers/OperatingConcepts/img/SolutionDiagram.png)


# Solution Development Walkthrough

Below is Track Docusign Recipients (TDR) solution development walkthrough. TDR tracks docusign envelopes or users and when a user doesn't perform wanted operation in given time, TDR notifies us with our selected notifier activity.

Here is initial UI structure of TDR.

![Track Docusign Recipients](/Docs/ForDevelopers/OperatingConcepts/img/TDRInitialState.png)


## Initial configuration

Track which Envelopes part in TDR UI is gathered to configure Monitor Docusign Envelope Activity, Time Picker control is used to configure Set Delay activity, dropdown below it is used to configure Test Incoming Data activity and notify dropdown is used to select and configure our notifier activity.

On initial configuration TDR creates all those UI and waits user to fill them. After user fills them TDR will create a complicated plan.

A developer should know what activities he/she is going to work with and create necessary UI components for those activities to create a solution like TDR.

TDR does some preperations on Initial Configuration to fill dropdowns and other UI components with necessary data.

###### Setting Docusign for configuration

    var configuration = DocuSignManager.SetUp(AuthorizationToken);

###### Loading templates from docusign and putting them in dropdown

    ActivityUI.TemplateSelector.ListItems.AddRange(DocuSignManager.GetTemplatesList(configuration).Select(x => new ListItem { Key = x.Key, Value = x.Value }));

###### Loading notifier activity templates from the Hub and putting them in dropdown

    ActivityUI.NotifierSelector.ListItems.AddRange((await HubCommunicator.GetActivityTemplates(Tags.Notifier, true)).Select(x => new ListItem { Key = x.Label, Value = x.Id.ToString() }));

###### Signalling DelayTime and ActionBeingTracked fields to use them in Message Builder activity

    CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RuntimeCrateLabel, true).AddField(DelayTimeProperty).AddField(ActionBeingTrackedProperty);

## Validate

When user starts configuring TDR and presses Build Solution button, Validate function of TDR is triggered. At this point TDR checks if all required information is entered by user and prevents FollowUp call if there are missing information.

    if (ActivityUI.BuildSolutionButton.Clicked)
      {
          if (ActivityUI.EnvelopeTypeSelectionGroup.Radios.All(x => !x.Selected))
          {
              ValidationManager.SetError("Envelope option is not selected", ActivityUI.EnvelopeTypeSelectionGroup);
          }
          if (ActivityUI.SentToSpecificRecipientOption.Selected)
          {
              ValidationManager.ValidateEmail(_configRepository, ActivityUI.SpecificRecipientEmailText);
          }
          if (ActivityUI.BasedOnTemplateOption.Selected)
          {
              ValidationManager.ValidateTemplateList(ActivityUI.TemplateSelector);
          }
          if (string.IsNullOrEmpty(ActivityUI.RecipientEventSelector.Value))
          {
              ValidationManager.SetError("Recipient action is not selected", ActivityUI.RecipientEventSelector);
          }
          if (string.IsNullOrEmpty(ActivityUI.NotifierSelector.Value))
          {
              ValidationManager.SetError("Forward action is not selected", ActivityUI.NotifierSelector);
          }
          if (ValidationManager.HasErrors)
          {
              ActivityUI.BuildSolutionButton.Clicked = false;
          }
      }
      return Task.FromResult(0);


After all validation checks are passed, FollowUp configuration of TDR is triggered.

## FollowUp Configuration

At followup phase TDR starts to create it's child and sibling activities which are necessary for a complete plan.

      //We need to keep the versions we know how to work with. If later these child activities will be upgraded we probably won't be able to configure them properly
      var activityTemplates = await HubCommunicator.GetActivityTemplates();
      var configureMonitorActivityTask = ConfigureMonitorActivity(activityTemplates);
      var configureSetDelayTask = ConfigureSetDelayActivity(activityTemplates);
      var configureQueryFr8Task = ConfigureQueryFr8Activity(activityTemplates);
      var configureTestDataTask = ConfigureFilterDataActivity(activityTemplates);
      ...

Those activties are created by getting activity templates from the Hub and requesting the Hub to configure them. After than it adds those activities as child or sibling activities. For details you can inspect TDR source code on [Github](https://github.com/Fr8org/Fr8Core/blob/master/terminalDocuSign/Activities/Track_DocuSign_Recipients_v2.cs)

Here is final completed UI after TDR finishes followup configuration with Publish to Slack selected as notifier.

![Track Docusign Recipients](/Docs/ForDevelopers/OperatingConcepts/img/TDRFinal.png)


[Go to Contents](/Docs/Home.md)
