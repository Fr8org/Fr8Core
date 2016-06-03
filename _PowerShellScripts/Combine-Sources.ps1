<#
    .SYNOPSIS
#>

param(
	[Parameter(Mandatory = $true)]
	[string]$github_username,

	[Parameter(Mandatory = $true)]
	[string]$github_password,

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

# $ErrorActionPreference = 'Stop'

$githubUrlBase = "https://{0}:{1}@github.com/Fr8org/" -f $github_username, $github_password

# Add more open source repos here
$openSourceRepos = @("terminalSlack")

# Append github URL to the repos
ForEach ($currentRepo in $openSourceRepos) { 
    Write-Host ("Processing {0}" -f $currentRepo)
    $currentRepoUrl = [System.IO.Path]::Combine($githubUrlBase, $currentRepo)
	$curTemp =[System.IO.Path]::Combine($tempDirectory, $currentRepo)
    $tempFileName = [System.IO.Path]::Combine($tempDirectory, $currentRepo + "_log.txt")

	if (Test-Path $curTemp) {
        cd $curTemp
		Remove-Item * -Force -Recurse
	}
    else {
	    md $curTemp
    	cd $curTemp
    }

	# Prevent git operation from failing (see http://stackoverflow.com/questions/11693074/git-credential-cache-is-not-a-git-command)
    Invoke-Expression  "git config --global credential.helper wincred"
	if ($LastExitCode -ne 0)
	{
		Write-Host ("Unable to configure git. Let's try to proceed..." -f $currentRepo)
	}


	# Clone open source repo
    Invoke-Expression  "git clone  $currentRepoUrl $curTemp 2> $tempFileName"

	if ($LastExitCode -ne 0)
	{
		Write-Host ("Failed to clone the repository {0}." -f $currentRepo)
		exit 1;
	}

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

    $curTerminal = [System.IO.Path]::Combine($sourcesDirectory, $currentRepo)

    if (Test-Path $curTerminal) {
        # Delete terminal from the main working directory (if exists)
        Remove-Item $curTerminal -Force -Recurse
    }

    # Re-create terminal subdirectory
    md $curTerminal

	# Move cloned directory with files to the main working directory (exclude .git folder)
    Robocopy $curTemp $curTerminal /S /XD .git

    exit 0;
} 