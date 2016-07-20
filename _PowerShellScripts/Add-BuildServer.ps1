#
# This script is intended to create a Build Server instance as an Azure VM.
# Before running the script you need to modify the first section to provide 
# the correct network configuration for the new machine. Then run it in the 
# PowerShell console e.g. Windows PowerShell ISE. 
#
# For details see: https://azure.microsoft.com/en-us/documentation/articles/virtual-machines-windows-capture-image/
#


$ErrorActionPreference = 'Stop'


$rgName = "BuildServers"
$storageAccName = "kwasabuildsfr8b041107350"
$number = Read-Host "Enter Build Server number"
$vmName = "fr8VMBuild-" + $number
$vmSize = "Standard_DS2"
$computerName = "fr8VMBuild-" + $number
$osDiskName = "System"
$location="West US"
$pipName = $vmName + '-PIP'
$nicname = $vmName + '-NIC'
$vnetName="BuildServers"
$subscriptionId = "74b73aa6-5c40-4a73-b29c-775a6cd3f49f"

Select-AzureRmSubscription -SubscriptionId $subscriptionId

# Uncomment to use existing network objects
#$pip = Get-AzureRmPublicIpAddress -Name $pipName -ResourceGroupName $rgName
#$vnet = Get-AzureRmVirtualNetwork -Name BuildServers -ResourceGroupName BuildServers
#$subnetconfig = Get-AzureRmVirtualNetworkSubnetConfig -VirtualNetwork $vnet
#$nic = Get-AzureRmNetworkInterface -Name "buildagent-01175" -ResourceGroupName $rgName

# Create new entwork objects (existing virtual network)
$pip = New-AzureRmPublicIpAddress -Name $pipName -ResourceGroupName $rgName -Location $location -AllocationMethod Dynamic
$vnet =  Get-AzureRmVirtualNetwork -Name $vnetName -ResourceGroupName $rgName
$nic = New-AzureRmNetworkInterface -Name $nicname -ResourceGroupName $rgName -Location $location -SubnetId $vnet.Subnets[0].Id -PublicIpAddressId $pip.Id


#Enter a new admin user name and password in the pop-up for the following
$cred = Get-Credential

#Get the storage account where the captured image is stored
$storageAcc = Get-AzureRmStorageAccount -ResourceGroupName $rgName -AccountName $storageAccName

#Set the VM name and size
$vmConfig = New-AzureRmVMConfig -VMName $vmName -VMSize $vmSize

#Set the Windows operating system configuration and add the NIC
$vm = Set-AzureRmVMOperatingSystem -VM $vmConfig -Windows -ComputerName $computerName -Credential $cred -ProvisionVMAgent -EnableAutoUpdate

$vm = Add-AzureRmVMNetworkInterface -VM $vm -Id $nic.Id

#Create the OS disk URI
$osDiskUri = '{0}vhds/{1}{2}.vhd' -f $storageAcc.PrimaryEndpoints.Blob.ToString(), $vmName.ToLower(), $osDiskName
$urlOfCapturedImageVhd = '{0}system/Microsoft.Compute/Images/buildserver/templ-osDisk.3d8c801f-b2a6-4650-a120-92cc88b23f56.vhd' -f $storageAcc.PrimaryEndpoints.Blob.ToString()

#Configure the OS disk to be created from image (-CreateOption fromImage) and give the URL of the captured image VHD for the -SourceImageUri parameter.
#We found this URL in the local JSON template in the previous sections.
$vm = Set-AzureRmVMOSDisk -VM $vm -Name $osDiskName -VhdUri $osDiskUri -CreateOption fromImage -SourceImageUri $urlOfCapturedImageVhd -Windows

#Create the new VM
New-AzureRmVM -ResourceGroupName $rgName -Location $location -VM $vm