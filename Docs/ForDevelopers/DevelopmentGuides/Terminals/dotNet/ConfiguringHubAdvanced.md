Configuration of the Fr8 Hub
========================

If you simply fire a Fr8 Hub up locally, its core should work with no additional require configuration beyond the setting of your master administrator account.

However, there are a number of essentially optional subsystems that require additional accounts or configuration. There are also a case or two where the default settings point to a single free account that may get noisy with the output of other Fr8 Developers, in which case you may want to change the settings to "go private".

The Fr8 Hub project uses web.config, of course, but we've set it up to look for an external file called settings.config for some settings. A sample of this file is available in a file in the root called settings.config.readme, found in HubWeb/Config/HubWeb. 

If you want to use any of these settings, update their values and save the file as settings.config, in the same location.  (i.e. remove the ".readme".)

Some Fr8 Subsystems will not work out of the box until these settings are configured. 

Roadmap: We've just created an Administration Settings Wizard to make this process easier but it's in an early stage.


The System User Account
------------------------

Fr8 carries out some internal processes via Fr8 Plans, and these Plans are executed in the context of a "system user". 
These configuration lines define this system user account"

```
<!-- System user account -->
<add key="SystemUserEmail" value="system1@fr8.co" />
<add key="SystemUserPassword" value="foobar" />
```

When the Administration Wizard is used to create a master administrative Fr8 account, those values are copied to these System user account settings
so the Plans run properly. One side effect of this is that your administrative account Activity Stream will fill up with a lot of 
notifcations related to internal plans. If you want to separate things out, just create another Fr8Admin account and use those values here.

Plan Directory Settings
-------------------------

The Plan Directory has search functionality. Fr8.co uses AzureSearch, and controls that via these configuration settings:

```
<add key="AzureSearchServiceName"
<add key="AzureSearchApiKey" value="8BECB1D404347BFC7BF536D03999CFE7" />
<add key="AzureSearchIndexName" value="plan-template-dev-index" />
```

As of now, Plan Directory search won't work unless a paid Azure Search resource is provided here. 

File Storage Settings
-------------------------
Some Activities like Load Excel File need Fr8 to store files and make them available. Fr8.co currently does this using Azure Storage, controlled
with the following configuration settings:

```
<!-- Azure storage -->
<add key="AzureStorageDefaultConnectionString" value="DefaultEndpointsProtocol=https;AccountName=yardstore1;AccountKey=[###]" />
<add key="DefaultAzureStorageContainer" value="default-container-dev" />
```

Email and SMS Delivery Settings
-------------------------
The Hub includes an email contact form. This is dispatched via these SendGrid-oriented settings:

```
<!--Email Delivery -->
<add key="OutboundEmailHost" value="smtp.sendgrid.net" />
<add key="OutboundEmailPort" value="587" />
<add key="OutboundUserName" value="[name]" />
<add key="OutboundUserPassword" value="[###]" />
<add key="ForgotPassword_template" value="760f0be0-6ccc-4d31-aeb1-297f86267475" />

<!-- Other settings -->
<add key="EmailAddress_GeneralInfo" value="info@fr8.co" />
<add key="DocumentationFr8Site_SMSLink" value="http://documentation.fr8.co/sms" />
```

Token Storage Settings
--------------------------
In some non-OAuth scenarios, the Hub needs to store user passwords. Fr8.co does this by using Azure Key Vault, with the following settings

```
<!-- Key Vault -->
<add key="AuthorizationTokenStorageMode" value="KeyVault" />
<add key="KeyVaultUrl" value="https://fr8Dev2KV.vault.azure.net:443" />
<add key="KeyVaultClientId" value="[###]" />
<add key="KeyVaultClientSecret" value="[###]" />
```

This is turned off by default. For more information look in settings.config.readme.

Notification Service Settings
---------------------------

The Hub relies on Pusher for notification services, and controls it with the following settings:

```
<!--Pusher -->
<add key="pusherAppId" value="148580" />
<add key="pusherAppKey" value="[####]" />
<add key="pusherAppSecret" value="[###]" />
```

Form Service Settings
----------------------------

The Hub uses Google Forms as part of some internal workflows and manages that with these settings:

```
<!-- Details of the Google form which is used to submit new crate manifests. -->
<add key="ManifestSubmissionFormUrl" value="[###]" />
<add key="ManifestSubmissionFormId" value="[###]" />
```

Most of these settings are unnecessary unless you really want to provide the full Fr8 experience to end users. The biggest exception is probably the Pusher
configuration. Without a free Pusher account, you're not going to see notifications in your client, so we highly recommend you set one up.
