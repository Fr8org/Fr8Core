<#
.SYNOPSIS
    This script uploads a file to FTP server.    
#>


param(
	[Parameter(Mandatory = $true)]
    [string]$localFile,

	[Parameter(Mandatory = $true)]
    [string]$remoteFile,

	[Parameter(Mandatory = $true)]
    [string]$username,

	[Parameter(Mandatory = $true)]
    [string]$password
)

$rootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$localFile = [System.IO.Path]::Combine($rootDir, $localFile)

# Create FTP Rquest Object
$ftpRequest = [System.Net.FtpWebRequest]::Create("$remoteFile")
$ftpRequest = [System.Net.FtpWebRequest]$ftpRequest
$ftpRequest.Method = [System.Net.WebRequestMethods+Ftp]::UploadFile
$ftpRequest.Credentials = new-object System.Net.NetworkCredential($username, $password)
$ftpRequest.UseBinary = $true
$ftpRequest.UsePassive = $true
# Read the File for Upload
$fileContent = gc -en byte $localFile
$ftpRequest.ContentLength = $fileContent.Length
# Get Stream Request by bytes
$run = $ftpRequest.GetRequestStream()
$run.Write($fileContent, 0, $fileContent.Length)
# Cleanup
$run.Close()
$run.Dispose()