#
# UpdateGitHubBuildStatus.ps1
#

param(
    [string]$buildNumber = $env:BUILD_BUILDNUMBER
)


$vso_username = ""
$vso_password = ""

$basicAuth = ("{0}:{1}" -f $vso_username,$vso_password)
$basicAuth = [System.Text.Encoding]::UTF8.GetBytes($basicAuth)
$basicAuth = [System.Convert]::ToBase64String($basicAuth)
$headers = @{Authorization=("Basic {0}" -f $basicAuth)}

$vsoResult = Invoke-RestMethod -Uri https://fr8.visualstudio.com/defaultcollection/fr8/_apis/build/builds/$buildNumber?api-version=2.0 -headers $headers -Method Get | ConvertFrom-Json

if ($vsoResult.result -ne "succeeded")
{
	
}