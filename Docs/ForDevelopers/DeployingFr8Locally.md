# Deploying Fr8 on the local PC

## Deploy to be hosted by a local IIS

This is most robust option of deploying Fr8 on your PC. All the job is done by the deployment script **DeployFr8Locally.ps1** located under **_PowerShellScripts** folder. This script does the following:  
1. Installs and configures IIS if required
2. Creates IIS Web sites
3. Configures the web site to point to the corresponding Fr8 component folder
4. Grants access to the DB for Fr8
5. Grants access to the files and folders for Fr8
6. Configures Visual Studio projects to use local IIS.


### Prerequisites  
1. You should have administrator access to your PC.
(http://www.howtogeek.com/112455/how-to-install-iis-8-on-windows-8/) or [here](https://technet.microsoft.com/en-us/library/hh831475(v=ws.11).aspx).
2. You should have enabled PowerShell script execution on your system. If you have never used PoweShell before then script execution is likely to be disabled. See [how to enable it](http://superuser.com/questions/106360/how-to-enable-execution-of-powershell-scripts).
3. You should be able to compile Fr8 successfully.

### Deployment guide

1. Close the Visual Studio.
2. Open PowerShell console under administrator account.
3. Navigate PowerShell to the cloned Fr8 repository root folder (this can be done using **cd** command or [Set-Location cmdlet](https://msdn.microsoft.com/en-us/powershell/scripting/getting-started/cookbooks/managing-current-location)).
4. Run deployment script by printing the following command: **.\\_PowerShellScripts\\DeployFr8Locally.ps1**

The following will happen:
* The script will recursively scan all folders using current PowerShell location as the root
* In each folder the script will try to find either **Terminal** project or **Hub**.
* For each found project a new site will be created. Each site consumes one port. Each site is binded to all available network interfaces. If you alredy use some of Fr8 ports, that bindings won't be rewritten, and the corresponding Fr8 component will be skipped.
* All sites will operate using one Application Pool with name **Fr8**. This application pool will use ApplicationPoolIdentity.
* The script will connect to SQL server using connection string found in the HubWeb project settings and **create any database** access will be granted for **Fr8** application pool identity. 
* If Fr8 database is already present, then **Fr8** application pool identity will become the **owner** of this database.
* Each found **Terminal** project or **Hub** project will be configured to use local IIS. This changes are made only for you and will be ignored by git.

### Advanced deployment settings
Default deployment settings are good for the most cases. But there are few setting that can be tweaked. 

#### Controlling application pools
You can rename application pool that is used for Fr8. **-globalAppPoolName** is responsible for that:

    .\\_PowerShellScripts\\DeployFr8Locally.ps1 -globalAppPoolName CustomFr8Poolname  

You can direct the deployment script to create an application pool per each Fr8 component. In this case application pool will be named in the standardized way. To enable multiple pools creation assign **-globalAppPoolName** to **$null**:  

    .\\_PowerShellScripts\\DeployFr8Locally.ps1 -globalAppPoolName $null  

#### Controlling application pool identity
By the default each application pool works under its own unique identity that is managed by IIS. You have an option to specify the user you want explicitly:

    .\\_PowerShellScripts\\DeployFr8Locally.ps1 -globalAppPoolName $null  -identityType 3 -userName "DOMAIN\USERNAME" -userPwd "type your password here"

Important to note that user name should always contains a domain specification. If your computer do not belong to any domain you should user your PC name as the domain.