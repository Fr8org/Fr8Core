[CmdletBinding()]
Param(
  [Parameter(Mandatory=$False,Position = 0)]
   [string]$globalAppPoolName = "fr8",

   # Possible identity types:
   # 0 - LocalSystem
   # 1 - LocalService
   # 2 - NetworkService
   # 3 - SpecificUser
   # 4 - ApplicationPoolIdentity	
   [Parameter(Mandatory=$False, Position = 1)]
   [int]$identityType = 4,

   [Parameter(Mandatory=$False, Position = 2)]
   [string]$userName,

   [Parameter(Mandatory=$False, Position = 3)]
   [string]$userPwd
)
  
  Import-Module WebAdministration
  
  # ************************************************************************************
  # Functions
  # ************************************************************************************

  function Grant-DbAccess
  {
    Param(
    
    [parameter(Mandatory=$true, Position=0)]
    [system.data.sqlclient.sqlcommand]
    $command,

    [parameter(Mandatory=$true, Position=1)]
    [string]
    $dbName,
    
    [parameter(Mandatory=$true, Position=2)]
    [string]
    $userName
    )
    
    $command.CommandText = "USE [master]
            IF NOT EXISTS  (SELECT name FROM master.sys.server_principals  WHERE name = '$userName')
            BEGIN
                CREATE LOGIN [$userName] FROM WINDOWS WITH DEFAULT_DATABASE=[master]
            END

            GRANT CREATE ANY DATABASE TO [$userName]

            select case when DB_ID('$dbName') is not null then 1 else 0 end"


     $dbExists = [bool]$command.ExecuteScalar();

     if ($dbExists)
     {
        $command.CommandText = "use [$dbName]
                               SELECT count(sp.name) FROM sys.server_principals sp
                               INNER JOIN sys.database_principals dp ON dp.sid = sp.sid
	                           INNER JOIN sys.database_role_members drm ON drm.member_principal_id = dp.principal_id
	                           INNER JOIN sys.database_principals r ON r.principal_id = drm.role_principal_id
	                           WHERE sp.name = '$userName'"

        $directMapping = [bool]$command.ExecuteScalar();

        if (-not $directMapping)
        {
            Write-Host "Granting access to $dbName for user $userName ..." 

            $command.CommandText = "use [$dbName]
                IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = '$userName')
	            BEGIN
					CREATE USER [$userName] FOR LOGIN [$userName] WITH DEFAULT_SCHEMA=[dbo]
                END

  	            ALTER ROLE [db_owner] ADD MEMBER  [$userName]"

            if ($command.ExecuteNonQuery() -ne -1)
            {
	            Write-Host "Failed to create SQL User [$userName] and granted access to the target database $dbName."
            }
            else
            {
                Write-Host "Successfully created SQL User [$userName] and granted access to the target database $dbName."
            }
        }
     }
  }
  
  # ************************************************************************************

  function Extract-ConnectionString 
  {
    Param(
    [parameter(Mandatory=$true, Position=0)]
    [xml]
    $webConfig 
    )
    

    if ([bool]$webConfig.configuration.connectionStrings -and ([bool]$webConfig.configuration.connectionStrings.add))
    {
        $connectionstringSetting = $webConfig.configuration.connectionStrings.add | Where-Object  {$_.GetAttribute("name") -eq "Fr8LocalDB"}
        
        if ([bool]$connectionstringSetting)
        {
            return $connectionstringSetting.GetAttribute("connectionString")
        }
    }

    return $null
  }

  # ************************************************************************************
  # Script body
  # ************************************************************************************

  if (-not ($identityType -gt -1 -and $identityType -lt 5))
  {
    Write-Host "Unsupported identity type: $identityType"
    return
  }

  $usedBindings = new-object 'System.Collections.Generic.HashSet[string]'
  $appPools = new-object 'System.Collections.Generic.HashSet[string]'
  $changedPools = new-object 'System.Collections.Generic.HashSet[string]'

   foreach ($site in (Get-ChildItem "IIS:\Sites"))
   {
        if (-not ($site.name -like "fr8_*"))
        {
            foreach ($binding in $site.Bindings.Collection)
            {
                $usedBindings.Add($binding.bindingInformation.ToLower()) | out-null
            }
        }
   }

  $configFiles = (Get-ChildItem . -include ("*.csproj")  -rec -ErrorAction SilentlyContinue )

  foreach ($file in $configFiles)
  {
       $content = [xml](Get-Content $file)
       
       foreach ($pg in $content.Project.PropertyGroup)
       {
         if ($pg.ProjectTypeGuids -match "349c5851-65df-11da-9384-00065b846f21")
         {
            $assemblyName = $content.Project.PropertyGroup | Where-Object  {[bool]$_.AssemblyName}

            $webConfigPath = "$($file.Directory)\web.config" 

            if (Test-Path $webConfigPath)
            {
                $url = $null;
                $tempCS = $null;

                #extract port setting from Web.config or check if predefined ones are applicable
                if ($assemblyName.AssemblyName -match "terminal")
                {
                    $webConfig = [xml](Get-Content $webConfigPath)
                    
                    $tempCS = (Extract-ConnectionString $webConfig)

                    if ($webConfig.configuration.appSettings.add.count -gt 0)
                    {
                        $endpointSetting = $webConfig.configuration.appSettings.add | Where-Object  {$_.GetAttribute("key") -match ".TerminalEndpoint"}

                        if ([bool]$endpointSetting)
                        {
                            $url = [System.Uri]$endpointSetting.GetAttribute("value")

                            if ($url.IsDefaultPort -or -not ($url.Host -like "localhost"))
                            {
                                continue 
                            }
                        }
                        else
                        {
                            continue
                        }
                    }
                }
                else 
                {
                    if ($assemblyName.AssemblyName -match "hubweb")
                    {
                        $url = [System.Uri] "http://localhost:30643"

                        # and also extract connection string
                        $webConfig = [xml](Get-Content $webConfigPath)
                        $tempCS = (Extract-ConnectionString $webConfig)

                        $connectionString  = $tempCS;
                        
                    }
                }
                

                if ([bool]$url)
                {
                    echo "$($file.Directory) -> $($assemblyName.AssemblyName) at  $($url.OriginalString)"    
                                
                    # Create Web Site
                    $sitePath = "IIS:\Sites\fr8_$($assemblyName.AssemblyName)"

                    if ([bool]$globalAppPoolName)
                    {
                        $appPoolName = $globalAppPoolName
                    }
                    else
                    {
                        $appPoolName = "fr8_$($assemblyName.AssemblyName)"
                    }

                    if ([bool]$tempCS)
                    {
                        $appPools.Add($appPoolName) | out-null
                    }

                    $skipAppPoolModification = false

                    if (-not (Test-Path $sitePath))
                    {
                        if (-not ([bool] ($usedBindings | Where-Object {$_ -like "*:$($url.Port):*" })))
                        {
                            New-Item $sitePath -physicalPath $file.Directory -bindings @{protocol="http";bindingInformation="*:$($url.Port):"}
                        }
                        else
                        {
                            echo "Port $($url.Port) is already used by other site on your IIS. No fr8 site will be created for this port, but I'll try to configure the project to use existing site"
                            $skipAppPoolModification = true
                        }
                    }
                    
                    if (-not $skipAppPoolModification)
                    {
                        # Create App Pool per terminal
                        if (-not (Test-Path "IIS:\AppPools\$appPoolName"))
                        {
                            New-Item "IIS:\AppPools\$appPoolName"
                        }

                        if ($changedPools.Add($appPoolName))
                        {
                            $pool = Get-Item "IIS:\AppPools\$appPoolName";
        
                            if ($identityType -eq 3)
                            {
                                $pool.processModel.userName = $userName;
                                $pool.processModel.password = $userPwd;
                            }

                            $pool.processModel.identityType = $identityType;
                            $pool | Set-Item

                            if (-not($pool.state -eq "Started"))
                            {
                                $pool.Start();
                            }
                        }

                        # Assign app pool to the site
                        Set-ItemProperty $sitePath -name applicationPool -value $appPoolName
                    }

                    # Configure project's user settings to use IIS
                    $userSettingsFile = "$($file.FullName).user";
                
                    if (Test-Path $userSettingsFile)
                    {
                        $userSettingsContent = [xml](Get-Content (Get-Item $userSettingsFile))
                        $useIISExpressSetting = $userSettingsContent.Project.PropertyGroup | Where-Object  {[bool]$_.UseIISExpress}
                    
                        if ([bool]$useIISExpressSetting)
                        {
                            $useIISExpressSetting.UseIISExpress = "False"
                        }
                                                        
                        foreach($ext in $userSettingsContent.Project.ProjectExtensions)
                        {
                            if ((-not [bool]$ext.VisualStudio) -or 
                                (-not  [bool]$ext.VisualStudio.FlavorProperties) -or 
                                (-not ($ext.VisualStudio.FlavorProperties.GetAttribute("GUID") -like "{349c5851-65df-11da-9384-00065b846f21}")))
                            {
                                continue
                            }

                            if ([bool]$ext.VisualStudio.FlavorProperties.WebProjectProperties)
                            {
                                if ([bool]$ext.VisualStudio.FlavorProperties.WebProjectProperties.UseIIS)
                                {
                                    $ext.VisualStudio.FlavorProperties.WebProjectProperties.UseIIS = "True"
                                }
                                else
                                {
                                    echo "Invalid user project settings: $($file.FullName).user. Close Visual Studio and try again"
                                }

                                if ([bool]$ext.VisualStudio.FlavorProperties.WebProjectProperties.IISUrl)
                                {
                                    $ext.VisualStudio.FlavorProperties.WebProjectProperties.IISUrl = "http://localhost:$($url.Port)"
                                }
                                else
                                {
                                    echo "Invalid user project settings: $($file.FullName).user. Close Visual Studio and try again"
                                }
                            }

                            break
                        }

                        $userSettingsContent.Save($userSettingsFile);
                    }
                    else
                    {
                        $userSettingsContent = [xml]("<?xml version=""1.0"" encoding=""utf-8""?>
                        <Project ToolsVersion=""14.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                            <PropertyGroup>
                                <UseIISExpress>False</UseIISExpress>
                                <IISExpressSSLPort />
                                <IISExpressAnonymousAuthentication />
                                <IISExpressWindowsAuthentication />
                                <IISExpressUseClassicPipelineMode />
                                <UseGlobalApplicationHostFile />
                            </PropertyGroup>
                            <ProjectExtensions>
                                <VisualStudio>
                                    <FlavorProperties GUID=""{349c5851-65df-11da-9384-00065b846f21}"">
                                        <WebProjectProperties>
                                            <StartPageUrl>
                                            </StartPageUrl>
                                            <StartAction>CurrentPage</StartAction>
                                            <AspNetDebugging>True</AspNetDebugging>
                                            <SilverlightDebugging>False</SilverlightDebugging>
                                            <NativeDebugging>False</NativeDebugging>
                                            <SQLDebugging>False</SQLDebugging>
                                            <ExternalProgram>
                                            </ExternalProgram>
                                            <StartExternalURL>
                                            </StartExternalURL>
                                            <StartCmdLineArguments>
                                            </StartCmdLineArguments>
                                            <StartWorkingDirectory>
                                            </StartWorkingDirectory>
                                            <EnableENC>True</EnableENC>
                                            <AlwaysStartWebServerOnDebug>True</AlwaysStartWebServerOnDebug>
                                            <UseIIS>True</UseIIS>
                                            <AutoAssignPort>True</AutoAssignPort>
                                            <DevelopmentServerPort>0</DevelopmentServerPort>
                                            <DevelopmentServerVPath>/</DevelopmentServerVPath>
                                            <IISUrl>http://localhost:$($url.Port)</IISUrl>
                                            <NTLMAuthentication>False</NTLMAuthentication>
                                            <UseCustomServer>False</UseCustomServer>
                                            <CustomServerUrl>
                                            </CustomServerUrl>
                                            <servers defaultServer="""">
                                                <server name=""SelfHostServer"" exePath="""" cmdArgs="""" url=""http://localhost:$($url.Port)/"" workingDir="""" />
                                            </servers>
                                        </WebProjectProperties>
                                    </FlavorProperties>
                                </VisualStudio>
                            </ProjectExtensions>
                        </Project>")

                        $userSettingsContent.Save($userSettingsFile);
                    }
                }
            }
         }
      }
  }
  
  #grant access to the DB

if ([bool]$connectionString)
{
    Write-host "Using connection string: $connectionString"

    $connection = new-object system.data.SqlClient.SQLConnection($connectionString)
    $command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
    $connection.Open()
    
    if ($identityType -eq 4)
    {
        foreach ($pool in $appPools)
        {
            Grant-DbAccess $command $connection.Database "IIS APPPOOL\$pool"
        }
    }
    
    if ($identityType -eq 3)
    {
        Grant-DbAccess $command $connection.Database $userName    
    }
}
else
{
    Write-host "Connection string is missing"
}


