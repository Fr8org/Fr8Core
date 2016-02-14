$RootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$Path= $RootDir+"\*\obj\Release"
$Handle = $RootDir+"\BuildUtils\handle.exe"
$files = Get-ChildItem $Path -Recurse | where {$_.extension -eq ".resources" -OR $_.FullName.Contains("Utilities\obj\Release")}

foreach($file in $files)
{
	[regex]$matchPattern = "(?<Name>\w+\.\w+)\s+pid:\s+(?<PID>\d+)\s+type:\s+(?<Type>\w+)\s+(?<User>\S+)\s+\w+:\s+(?<Path>.*)"

	$data = &$handle -u $file -silent
	$MyMatches = $matchPattern.Matches( $data )

	if ($MyMatches.count) {

		$MyMatches | foreach {
			Stop-Process $_.groups["PID"].value -passThru -Force -Verbose
		}
	}
}
