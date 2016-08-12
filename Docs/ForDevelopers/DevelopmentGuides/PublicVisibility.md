[Back](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/DevelopmentGuides/PlatformIdependentTerminalDeveloperGuide.md)

## Making Your Terminal Visible on the Public Web

NOTE: This is only necessary if you choose, as your [development approach](./ChoosingADevelopmentApproach.md), to test against the public Fr8 Hub, or if you want to go into production and host your own production Terminal.

To work with the Hubs at [dev.fr8.co](http://dev.fr8.co) or [fr8.co](http://fr8.co) you must provide publically accessible HTTP endpoint. Ideally, DNS address of this endpoint should not change through the time, otherwise you'll have to register your terminal everytime endpoint is changed. Fr8 doesn't restrict terminal endpoint to be HTTPS.

So here are a few options to go with:

* Hosting providers
* Dynamic DNS services
* ngrok (this is what the core Fr8 team uses. It's free and easy, although we actually pay for the pro service so we can have permanent public ips)
* Fr8 doesn't restrict terminal endpoints to have DNS name. So a publically accessible static IP will work too.

Before your terminal can be used with any Hub, it has to be registered. In order to register your terminal, do the following: 
* In the top Hub menu, choose **Tools** and then **Show Developer Menu**
* Select **My Terminals** in the Developer menu
* Click on the **Add Terminal** button. 
* Specify **Development URL** for your terminal and click Ok. If the terminal is available and can be successfully discovered, it will be added on the Hub with the Unapproved state.

If you're registering your terminal on Fr8.co or another public Hub, you will be able to start testing your terminal right away. However, other Fr8.co users won't be able to use the terminal until it is approved by the Fr8 team. In order to get your terminal approved, contact Fr8 Support and provide the production URL for the terminal (it can be the same as the development URL). The terminal should propertly respond to the basic requests from the Hub according to the Fr8 specifications. 

As soon as your terminal is approved, it becomes accessible for all Fr8 users.

If you're running the Hub locally and logged in as Fr8 Administrator, you can approve your terminal by clicking on the newly added terminal on the **My Terminal page** and checking the **Approve terminal** checkbox. You will need to confirm approval when you click on Save. 
