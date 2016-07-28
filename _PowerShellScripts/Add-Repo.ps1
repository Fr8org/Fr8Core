<#
    .SYNOPSIS
#>

param(
	[Parameter(Mandatory = $true)]
	[string]$github_username,

	[Parameter(Mandatory = $true)]
	[string]$github_password,

	[Parameter(Mandatory = $true)]
	[string]$github_account,

	[Parameter(Mandatory = $true)]
	[string]$github_repo,

    [string]$branchName = "dev",

	[string]$sourcesDirectory = $env:BUILD_SOURCESDIRECTORY,

    [string]$tempDirectory = $env:BUILD_STAGINGDIRECTORY
)

function Checkout-Branch($branchName) {
    # Checkout the specified branch 
    Invoke-Expression "git checkout $branchName 2> $tempFileName"
    if ($LastExitCode -ne 0)
    {
	    return $false;
    }
    return $true;
}

if (Test-Path $tempDirectory) {
    cd $tempDirectory
	Remove-Item * -Force -Recurse
}
else {
	md $tempDirectory
    cd $tempDirectory
}

$githubUrl = "https://{0}:{1}@github.com/{2}/{3}.git" -f $github_username, $github_password, $github_account, $github_repo

$tempFileName = [System.IO.Path]::Combine($tempDirectory, "log.txt") 
$checkoutDirectory = [System.IO.Path]::Combine($tempDirectory, "checkout")

# Prevent git operation from failing (see http://stackoverflow.com/questions/11693074/git-credential-cache-is-not-a-git-command)
Invoke-Expression  "git config --global credential.helper wincred"
if ($LastExitCode -ne 0)
{
	Write-Host ("Unable to configure git. Let's try to proceed..." -f $currentRepo)
}

# Clone open source repo
Invoke-Expression "git clone  $githubUrl $checkoutDirectory 2> $tempFileName" 

if ($LastExitCode -ne 0)
{
	Write-Host ("Failed to clone the repository {0}." -f $currentRepo)
	exit 1;
}

Echo "Cloned successfully"
cd $checkoutDirectory

# Checkout the actual branch: try the specified branch and fallback to master 
if (-not(Checkout-Branch ($branchName))){
    if (-not(Checkout-Branch ("master"))){
	    Write-Host "Failed to checkout the branch $branchName or master, existing."
        exit 1;
    }  
    else
    {
        Write-Host ("The branch {0} cannot be checked out, checked out master instead." -f $branchName)
    }
}

echo "Branch has been checked out"

# Move cloned directory with files to the main working directory (exclude .git folder)
Robocopy $checkoutDirectory $sourcesDirectory /S /XD .git

exit 0;