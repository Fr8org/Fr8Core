# Adding OAuth Support to a Terminal

Not all Terminal Activities require authentication, but if you need access to a user's accounts from some Web Service, you probably need to support the OAuth process in your Terminal. Put another way: if you respond to a /discover request from the Hub with at least one ActivityTemplate that indicates Authentication is required, then your Terminal will need to support the [two /authentication endpoints](http://dev-terminals.fr8.co:25923/swagger/ui/index#/Authentication).

This material assumes you've got a good basic understanding of OAuth. If you need to learn, we recommend [this](https://aaronparecki.com/articles/2012/07/29/1/oauth2-simplified).

There are two elements of OAuth support you need to consider: 
1) The specific implementation supported by the Web Service your Terminal is connecting to
2) Participating in the Fr8 authorization interaction

We'll focus on #2. By looking at existing Terminal source code, you'll be able to see a number of different appraoches to #1. There are often platform-specific SDK's that simplify the interaction with a particular Web Service.


The Basic Fr8 OAuth Interaction
--------------------------------------

This is diagrammed [here](/Docs/ForDevelopers/OperatingConcepts/Authorization/AuthOverview.md). The key items are:

1. If your ActivityTemplate is signalling that Authentication is required, and the Fr8 Hub can't find a valid Authorization Token, it will [POST to the Terminal](http://dev-terminals.fr8.co:25923/swagger/ui/index#!/Authentication/Authentication_GenerateOAuthInitiationURL) to retrieve the **initialOAuthUrl**
2. You return this URL to the Hub. It's usually hardcoded into your Terminal. 
3. Fr8 redirects the user to this URL, triggering the OAuth process, and if all goes well receives a http response with a code.
4. The code gets passed first from the Client to the Hub, and the Hub then passes the code to the Terminal by [POSTing to /authentication/token](https://fr8.co/swagger/ui/index#!/Authentication/Authentication_token). 
5. Terminal gets **code** from a response and contacts the Web Service to exchange it for an oAuth2 **access_token**. (for example, the Google Terminal does this with the line var oauthToken = _googleIntegration.GetToken(code). 
6. The Terminal returns the retrieved token to the Hub, which stores it with the Fr8 User account.
7. Depending on the intial circumstance, the Hub may redirect to the Client to enable the user to continue with what they were doing.

As a Terminal developer you have to implement responses to these requests, and carry out whatever steps are required by any Web Services you're attempting to connect to.

###Finding the Initial OAuth URL

This process varies from Web Service to Web Service. Here are a couple of examples.
Both MailChimp and Slack ask you to register a new "app" and then provide you with the information necessary to construct the initial URL.

| Mail Chimp app registration   |      Slack app registration      |
|----------|:-------------:|
|![](/Docs/img/TerminalDeveloping-Authentication.md-1.png) |  ![](/Docs/img/TerminalDeveloping-Authentication.md-2.png) | 

After that you should be able to compose an **initialOAuthUrl**

It might look like this: 
`https://slack.com/oauth/authorize?client_id=9815816992.14932358039&amp;state=%STATE%&amp;scope=client&amp;redirect_uri=http://localhost:30643/AuthenticationCallback/ProcessSuccessfulOAuthResponse?terminalName=terminalSlack%26terminalVersion=1`
or this: 
`https://login.mailchimp.com/oauth2/authorize?response_type=code&amp;state=%STATE%&amp;client_id=583227558154&amp;redirect_uri=http%3A%2F%2F127.0.0.1%3A30643%2FAuthenticationCallback%2FProcessSuccessfulOAuthResponse%3FterminalName%3DterminalMailChimp%26terminalVersion%3D1`

**redirect_uri** is where the user will be redirected after authentication. Currently we use URL's like `http://localhost:30643/AuthenticationCallback/ProcessSuccessfulOAuthResponse?terminalName=terminalName&terminalVersion=1`

**Note 1**: In order to use "&" in .conifg file you have to replace it with "&amp;"
**Note 2**: Make sure, that  redirect_uri is HTTP encoded


Encoded: ` http://localhost:30643/AuthenticationCallback/ProcessSuccessfulOAuthResponse?terminalName=terminalSlack%26terminalVersion=1 `
Not encoded: ` http://localhost:30643/AuthenticationCallback/ProcessSuccessfulOAuthResponse?terminalName=terminalSlack&terminalVersion=1 `

###Example: Generating the initial oAuth url
In this C# example, the Terminal has built an  AuthenticationController which responds to the incoming POST request. 

![](/Docs/img/TerminalDeveloping-Authentication.md-3.png)

The hardcoded URL is stored as a configuration setting, and retrieved with CloudConfigurationManager.GetSetting("MailChimpOAuthUrl") 
`<add key="MailChimpOAuthUrl" value="https://login.mailchimp.com/oauth2/authorize?response_type=code&amp;state=%STATE%&amp;client_id=583227558154&amp;redirect_uri=http%3A%2F%2F127.0.0.1%3A30643%2FAuthenticationCallback%2FProcessSuccessfulOAuthResponse%3FterminalName%3DterminalMailChimp%26terminalVersion%3D1" />`
A GUID is injected into the URL to make it unique, and it's returned to the user via C# class called ExternalAuthUrlDTO which automates the JSON serialization.

If you have composed  **initialOAuthUrl** right, then user will be redirected to 3rd party authentication page and after user enters their credentials **redirect_uri** will be called with code and state parameters

At this point the AuthenticationController has to implement a method to respond to the [POSTing to /authentication/token](https://fr8.co/swagger/ui/index#!/Authentication/Authentication_token). 

### Generating oAuthToken


In order to generate **oAuthToken** you have to make a call to OAuthAccessUrl with the code you've received
First you have to parse received **code** and **state**

![](/Docs/img/TerminalDeveloping-Authentication.md-4.png)

When you get **code** and **state** you have to make a call to **OAuthAccessUrl**
Different services have different variations on how this is done, so read their documentation carefully. Here are two different examples:


#### Example: Getting a Token from Slack

In Slack, in order to exchange a code for a token you simply have to make a GET call on the **OAuthAccessUrl** with code specifed.

![](/Docs/img/TerminalDeveloping-Authentication.md-5.png)

#### Example: Getting a Token from MailChimp

In MailChimp you have to do it differently.

You have to:
1. Make a POST call to https://login.mailchimp.com/oauth2/token
2. Specify response_type in addition to **code, client_id,  client_secret and  redirect_uri** as a POST call payload
3. Specify special User-Agent 
4. Decompress the response

Code for MailChimp GetOAuthToken looks like this:

![](/Docs/img/TerminalDeveloping-Authentication.md-6.png)

 

###Troubleshooting:
- Pay attention to API documentation
- Make sure your redirect URL is HTTP encoded
- Make sure your redirect URL is equivalent everywhere
- Use Fiddler to check if your request exactly matches the one specified in documentation


###WebService
After you are done with Authentication you have to configure the [WebService](TerminalDeveloping-AddingAWebService.md) 


