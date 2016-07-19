### Terminal Registration

Registering your Terminal with a Hub causes the Hub to:
1) send a /discover request to your Terminal immediately, and each time its starts up thereafter
2) use the response to that /discover request to add your Terminal's Activitiees to the set of Activities made available to client users of that Hub.

Go to either  [fr8.co](https://fr8.co/dashboard/terminals) (if you've chosen the ["Public Hub" Development Approach](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/ChoosingADevelopmentApproach.md)), or to the /dashboard/terminals page on your local running Hub (if you've chosen the Local Hub Development Approach)
and do the following:

1) Figure out the port number your local Terminal is using, and the resulting URL, which is usually something like localhost:38405.

2) Click "Add Terminal" and add the URL of your new Terminal

Note that you must use exactly the same endpoint that your terminal returns in the response to **/discover** request.

Following this, the Hub will make a [GET Discover call to your Terminal](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/Guide-TerminalDiscovery.md), integrating your Activities into its pool of available Activities.

You can now open Plan Builder (found at https://fr8.co/dashboard/plans or at your local equivalent) and see your new activity in activity selection pane. 

You can even try to add your activity to the plan, but unless you've prepared your Activity to respond to /configure,  activity configuration will fail. 
