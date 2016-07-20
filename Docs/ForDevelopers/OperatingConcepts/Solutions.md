# Solutions

Fr8 Solutions are a set of preconfigured activities. It helps users to create preconfigured complex plans.

## Step 1 - Solution Configuration

Let's assume you want to load data from Activity A (AA), process that data using Activity B (AB) and output that data to Activity C (AC). A newbie user might have hard time putting all those activities in a meaningful order with correct configuration. Solutions get in scene at this point. Let's call our solution Solution ABC. Solution ABC's UI will have necessary fields to gather information about AA, AB and AC. When user starts to fill required information Solution's configuration gets triggered. On this stage solution checks if all required configuration is ready.

## Step 2 - Child Activity Creation and Configuration

If user filled necessary information, solution loads all activity templates from HUB. Finds ActivityTemplate of A. Requests HUB to configure it. After HUB's response we have complete structure of AA. Assuming we know how AA works - we find UI crate of AA. Modify it's controls according to our target and according to user inputs on our solution. We do same operation for AB and AC. After modifying all Activities we add those activities as our children.

We return our ActivityDTO to HUB and voila - user sees Solution ABC with AA,AB and AC as children and preconfigured for our target.

# Flow

![alt tag](https://raw.githubusercontent.com/Fr8org/Fr8Core/master/Docs/ForDevelopers/OperatingConcepts/img/SolutionDiagram.png)
