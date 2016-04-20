<#
    .SYNOPSIS
    The script copies all blobs from the specified container in one Azure storage account 
	to another container in another Azure storage account. The two accounts do not have to be 
	in one Azure subscription.
#>


#Change these to match the source account
$sourceStorageAccountName = "buildservers6531" 
$sourceStorageKey = Read-Host "Paste key for storage account $sourceStorageAccountName"
$sourceContainerName = "system"

#Change these to match the destination account  
$destStorageAccountName = "kwasabuildsfr8b041107350"
$destStorageAccountKey = Read-Host "Paste key for storage account $destStorageAccountName"
$destContainerName = "images"
  
$sourceContext = New-AzureStorageContext -StorageAccountName $sourceStorageAccountName -StorageAccountKey $sourceStorageKey -Protocol Http
$destContext = New-AzureStorageContext -StorageAccountName $destStorageAccountName -StorageAccountKey $destStorageAccountKey
  
# Copy Operation
$blob1 = Get-AzureStorageBlob `
    -Context $sourceContext `
    -Container $sourceContainerName | 
    ForEach-Object `
    {
       Write-Host $_.Name
       $destBlob = $([System.IO.Path]::GetFileName($_.Name))

       Start-AzureStorageBlobCopy `
        -SrcContext $sourceContext `
        -SrcContainer $sourceContainerName `
        -SrcBlob "$($_.Name)" `
        -DestContext $destContext `
        -DestContainer $destContainerName `
        -DestBlob "$destBlob" `
    }