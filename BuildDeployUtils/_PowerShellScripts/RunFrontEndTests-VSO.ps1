# Create an IIS website to server test harness page

function Get-ScriptDirectory {
    Split-Path -parent $PSCommandPath
}
$scriptDir = Get-ScriptDirectory
# Locate Chutzpah
$ChutzpahDir = get-childitem "$scriptDir\..\packages\" chutzpah.console.exe -recurse | select-object -first 1 | select -expand Directory
Write-host "$($ChutzpahDir)"

# Run tests using Chutzpah and export results as JUnit format to chutzpah-results.xml

$ChutzpahCmd = "$($ChutzpahDir)\chutzpah.console.exe .\Scripts\tests\unit /junit .\chutzpah-results.xml"
Write-Host $ChutzpahCmd
Invoke-Expression $ChutzpahCmd

# Parse test results 

$testsuites = [xml](get-content .\chutzpah-results.xml)
 
$anyFailures = $FALSE
foreach ($testsuite in $testsuites.testsuites.testsuite) {
    write-host " $($testsuite.name)"
    foreach ($testcase in $testsuite.testcase){
        $failed = $testcase.failure
        # $time = $testsuite.time
        if ($testcase.time) { $time = $testcase.time }
        if ($failed) {
            write-host "Failed   $($testcase.name) $($testcase.failure.message)"
            # Add-AppveyorTest $testcase.name -Outcome Failed -FileName $testsuite.name -ErrorMessage $testcase.failure.message -Duration $time
            $anyFailures = $TRUE
        }
        else {
            write-host "Passed   $($testcase.name)"
            # Add-AppveyorTest $testcase.name -Outcome Passed -FileName $testsuite.name -Duration $time
        }
    }
}

if ($anyFailures -eq $TRUE){
    write-host "Failing build as there are broken tests"
    $host.SetShouldExit(1)
}