### Terminal Registration

NOTE: You only need to register your Terminal if you want a public Hub like fr8.co to import its capabilities, enabling you to use that Hub's accounts and client to access your Terminal
You'll only want to do that if you've chosen the "Public Hub" Development Approach. If you're using "Local Hub" Development 

Now you can go to either  [fr8.co](https://fr8.co/dashboard/terminals) (if you've chosen the ["Public Hub" Development Approach](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/ChoosingADevelopmentApproach.md)), or to the /dashboard/terminals page on your local running Hub (if you've chosen the Local Hub Development Approach)
and follow the instructions here [link to the page with terminal registration instructions].
For registration process you must use exactly the same endpoint that your terminal returns in the response to **/discover** request.

After terminal registration succeeds, the Hub will make a [GET Discover call to your Terminal](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/Guide-TerminalDiscovery.md), integrating your Activities into its pool of available Activities.
You can now open Plan Builder (found at https://fr8.co/dashboard/plans or at your local equivalent) and see your new activity in activity selection pane. 

You can even try to add your activity to the plan, but unless you've prepared your Activity to respond to /configure,  activity configuration will fail. 
