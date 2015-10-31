#
# CheckWebSettings.ps1
#
cd $PSScriptRoot
cd ..
$cur = (Get-Item -Path ".\" -Verbose).FullName

$projectFiles = get-childitem  -Include HubWeb.csproj, terminal*.csproj -Exclude terminalWebRole.csproj -recurse  -ErrorAction SilentlyContinue -Force | select -expandproperty FullName
foreach ($projectFile in $projectFiles) {
	$xml = [xml] (get-content $projectFile)
	$nsmgr =new-object System.Xml.XmlNamespaceManager($xml.NameTable)
	$nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003")
	$settings = $xml.SelectSingleNode("/ns:Project//ns:SaveServerSettingsInUserFile", $nsmgr).InnerText
	if ($settings -eq "False")
	{
		Write-Host  "Web settings are overwritten in project file: $projectFile." -ForegroundColor "Red"
		$failure = 1
	}
}

if ($failure -eq 1) {
	Write-Host "Web settings check FAILED" -ForegroundColor "White" -backgroundColor "Red" 
	Write-Host "Please uncheck 'Apply server settings to all users' on the Project settings dialog for the projects listed above and commit again."
}
else {
	Write-Host "Web settings check OK" -ForegroundColor "Green"
}
exit 0