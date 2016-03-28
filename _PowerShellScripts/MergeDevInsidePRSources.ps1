#
# MergeDevInsidePRSources.ps1
#

$devBranchRef = "refs/heads/dev"
$buildBranchName = "dev+$env:BUILD_SOURCEBRANCHNAME"
$buildSourcePath = "$env:BUILD_SOURCESDIRECTORY\build_source\"

if (Test-Path  $buildSourcePath)	
{
	Write-Host "Build Path directory already exists."
	#Write-Host "Making clean up..."
}
else
{
	Write-Host "Creating dirrectory for dev branch..."
	New-Item -ItemType directory -Path $devBranchPath
}

Write-Host "Getting dev branch from GitHub repo: $devBranchRef"
git checkout $devBranchRef

Write-Host "Creating new branch: $buildBranchName"
git checkout-index -a -f --prefix=$buildSourcePath

cd $buildSourcePath

Write-Host "Merging $devBranchRef into new branch: $buildBranchName"
git merge $env:BUILD_SOURCEBRANCHNAME

Write-Host "Merging $devBranchRef into new branch: $buildBranchName"
git merge $env:BUILD_SOURCEBRANCHNAME


