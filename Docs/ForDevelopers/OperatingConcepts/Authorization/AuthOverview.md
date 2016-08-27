Authentication Overview
=======================


Many Activities will require authorization. That is, it will be necessary for that activity to have a set of credentials to some external service.

All authentication is managed by the Hub. No passwords or tokens should ever be added to Crates that are stored in Activities or Containers.

Terminals signal a need for authentication by setting the value of the AuthenticationType property of each Template.

AuthenticationType
------------------

Terminals have a property called AuthenticationType which can be assigned with one of following enum values:

* None – action does not require authentication.
* Internal – This is a username/password process that is used for authentication processes that don't support OAuth. For these cases, Fr8 will render its own username/password dialog box, collect the data, store it, and pass it to the Terminal via the /authentication/internal endpoint, so the Terminal can carry out the non-Oauth authentication process. Like any well-behaved web service, Fr8 actually wants nothing to do with your passwords, so wherever possible, use OAuth/token-based approaches and treat this mechanism as deprecated.   
* External – This is classic, standard, OAuth 2.0, and the preferred solution. Credential information is collected from the user via a web page generated not by Fr8 but by the involved Web Service.
* InternalWithDomain – This is a variation on Internal that was created to enable the Atlassian terminal, which requires an additional string called Domain and doesn't support OAuth at this time. The  Fr8 dialog displays additional “Domain” field. 



The OAuth ("External") Authentication flow is shown below:

![external-authentication](https://github.com/Fr8org/Fr8Core/blob/master/Docs/img/AuthorizationExternalAuthentication.png)


Authentication flow based on Internal (or InternalWithDomain) AuthenticationType is shown below:

![internal-authentication](https://github.com/Fr8org/Fr8Core/blob/master/Docs/img/AuthorizationInternalAuthentication.png)

When a User creates or uploads a Plan to a Hub, the Hub will attempt to Activate the Plan. As part of this process, it examines each Activity in the Plan for the presence of a non-blank AuthenticationType. For each such Activity it checks its database for an AuthenticationToken associated with the requested Activity.
