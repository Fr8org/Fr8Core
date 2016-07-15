param(
    [Parameter(Mandatory = $true)]
	[string]$buildId,

    [Parameter(Mandatory = $true)]
	[string]$commitId
)

$template = "
` <html xmlns='http://www.w3.org/1999/xhtml'>
` <head>
`    <title></title>
` </head>
` <body>
`    This web application was deployed from the <a href='https://fr8.visualstudio.com/DefaultCollection/fr8/_build?_a=summary&buildId={0}'>build #{0}</a>. Commit id is <a href='https://github.com/Fr8org/Fr8Core/commit/{1}'>{1}</a>.
` </body>
` </html>
` "

$rootDir = Split-Path -parent (Split-Path -parent $myInvocation.MyCommand.Path)

ls $rootDir "*.csproj" -Recurse -File -Name | ? { $_ -inotmatch 'Tests' } | ForEach-Object {
    $path = 
    $path = $rootDir + "\" + (Split-Path $_ -Parent) + "\ver.html"
    Echo ($path)
    if (-not (Test-Path $path)) {
        New-Item $path -ItemType file 
    }

    Set-Content $path -Value ("$template" -f $buildId, $commitId)
}