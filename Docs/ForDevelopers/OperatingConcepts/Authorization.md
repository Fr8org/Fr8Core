# FR8 SECURITY – AUTHORIZATION
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  

Many Activities will require authorization. That is, it will be necessary for that activity to have a set of credentials to some external service.

All authentication is managed by the Hub. No passwords or tokens should ever be added to Crates that are stored in Activities or Containers.

Terminals signal a need for authentication by setting the value of the AuthenticationType property of each Template.

Terminals have a property called AuthenticationType which can be assigned with one of following enum values:

* None – action does not require authentication.
* Internal – action requires authentication, and authentication is performed with internal Fr8 dialog. Hub calls terminal’s /authentication/internal endpoint, which performs authentication with remote service internally.
* External – action requires authentication, and authentication is performed by opening external service OAuth page.
* InternalWithDomain – similar with “Internal” scenario. Action requires authentication, and authentication is performed with internal Fr8 dialog, which displays additional “Domain” field. Hub calls terminal’s /authentication/internal endpoint, which performs authentication with remote service internally, passing extra “Domain” property (some services do require additional domain field).

Authentication flow based on Internal (or InternalWithDomain) AuthenticationType is shown below:

![internal-authentication](https://github.com/Fr8org/Fr8Core.NET/blob/master/img/AuthorizationInternalAuthentication.png)

Authentication flow based on External AuthenticationType is shown below:

![external-authentication](https://github.com/Fr8org/Fr8Core.NET/blob/master/img/AuthorizationExternalAuthentication.png)


When a User creates or uploads a Plan to a Hub, the Hub will attempt to Activate the Plan. As part of this process, it examines each Activity in the Plan for the presence of a non-blank AuthenticationType. For each such Activity it checks its database for an AuthenticationToken associated with the requested Activity.

[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)  
