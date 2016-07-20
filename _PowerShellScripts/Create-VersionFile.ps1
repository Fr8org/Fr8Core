param(
    [Parameter(Mandatory = $true)]
	[string]$buildId,

    [Parameter(Mandatory = $true)]
	[string]$commitId
)

$ErrorActionPreference = 'Stop'

Add-Type -Path "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.Xml.Linq.dll"

$template = "
` <html xmlns='http://www.w3.org/1999/xhtml'>
` <head>
`    <title>Version information</title>
`    <meta http-equiv='Cache-Control' content='no-cache, no-store, must-revalidate'/>
`    <meta http-equiv='Pragma' content='no-cache'/>
`    <meta http-equiv='Expires' content='0'/>
` </head>
` <body>
`    This web application was deployed from the <a href='https://fr8.visualstudio.com/DefaultCollection/fr8/_build?_a=summary&buildId={0}'>build #{0}</a>. Commit id is <a href='https://github.com/Fr8org/Fr8Core/commit/{1}'>{1}</a>.
` </body>
` </html>
` "

$rootDir = Split-Path -parent (Split-Path -parent $myInvocation.MyCommand.Path)
$fileName = "ver.html"

ls $rootDir "*.csproj" -Recurse -File -Name | ? { $_ -inotmatch 'Tests' } | ForEach-Object {

    # Creating the version file
    $path = $rootDir + "\" + (Split-Path $_ -Parent) + "\" + "$fileName"
    $projectPath = $rootDir + "\" + $_
    if (-not (Test-Path $path)) {
        New-Item $path -ItemType file 
    }

    Echo "Processing project $projectPath"
    Echo "Writing file: $path"
    Set-Content $path -Value ("$template" -f $buildId, $commitId)

    # Update project file to get version files deployed
    [System.Xml.Linq.XNamespace] $ns = "http://schemas.microsoft.com/developer/msbuild/2003";

    $project = [System.Xml.Linq.XDocument]::Load($projectPath);
    $itemGroupNode = $project.Descendants($ns + "Project")[0].Descendants($ns + "ItemGroup")[0]

    # See if already added
    $exists = ($itemGroupNode.Descendants($ns + "Content") | Where-Object  { ($_.Name.LocalName -eq "Content") -and ($_.FirstAttribute.Value -eq $fileName ) } | Select Name) -ne $null
    if (-not $exists) {
        Echo "Adding reference to $fileName"
        $contentNode = new-object System.Xml.Linq.XElement(($ns + "Content"), 
        `  (new-object System.Xml.Linq.XAttribute("Include", $fileName)))
        $itemGroupNode.Add($contentNode)
        $project.Save($projectPath) 

        # Disable cache (if web project) 
        $configPath = $rootDir + "\" + (Split-Path $_ -Parent) + "\web.config"
        if (Test-Path $configPath) {
        Echo "Configuration file file found"
            $xml = [xml](Get-Content $configPath)
            $location = $xml.CreateElement("location")
            $location.SetAttribute(“path”,$fileName);
            $location.InnerXml = "<system.webServer><staticContent><clientCache cacheControlMode='DisableCache'/></staticContent></system.webServer>"
            $xml.configuration.AppendChild($location)
            $xml.Save($configPath)
        }
    }    
    else
    {
        Echo "Reference to $fileName already exsiting in the project file, skipping"
    }

    Echo ""
} 