set /P version=Enter package version:

%~dp0..\.nuget\NuGet.exe pack %~dp0Fr8.PrivateSettings.nuspec -OutputDirectory %~dp0 -Version %version%
%~dp0Tools\NuGet.exe push %~dp0Fr8.PrivateSettings.%version%.nupkg -Source https://fr8.pkgs.visualstudio.com/DefaultCollection/_packaging/fr8-private/nuget/v3/index.json -ApiKey VST

set /P version=Press Enter to close