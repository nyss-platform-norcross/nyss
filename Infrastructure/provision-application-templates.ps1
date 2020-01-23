<#
 .SYNOPSIS
    Deploys CBS nyss application to Azure

 .DESCRIPTION
    Deploys CBS nyss application using Azure Resource Manager template including parameters from selected environment

 .PARAMETER subscriptionId
    Optional. The subscription id where the template will be deployed.

 .PARAMETER complete
    Optional. Indicates if you want the resource deployment to wipe all resources in the resource group and create them again or incrementally, only creating the new resources. Incremental is default.

 .PARAMETER environment
    The environment name.
#>

param(
  [string] $subscriptionId,
  [Parameter(Mandatory = $true)][string] $environment,
  [Parameter(Mandatory = $false)][string] $specificResource = "all",
  [switch] $complete,
  [switch] $test
)

$ErrorActionPreference = "Stop"
$resourceGroupName = "nrx-cbs-$environment-rg"
$AzModuleVersion = "2.0.0"

# Verify that the Az module is installed
if (!(Get-InstalledModule -Name Az -MinimumVersion $AzModuleVersion -ErrorAction SilentlyContinue)) {
  Write-Host "This script requires to have Az Module version $AzModuleVersion installed..
It was not found, please install from: https://docs.microsoft.com/en-us/powershell/azure/install-az-ps"
  exit
}

if ($subscriptionId) {
  # sign in
  Write-Host "Logging in...";
  Connect-AzAccount;

  # select subscription
  Write-Host "Selecting subscription '$subscriptionId'";
  Select-AzSubscription -SubscriptionId $subscriptionId;
}

#Check that resource group exists
$resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue

if (!$resourceGroup) {
  Write-Error "Resource group $resourceGroupName not found!"serviceBusNamespaceName
}

if ($complete) {
  if ($test){
    Write-Host "Test deploying all resources (Complete mode)"
    Test-AzResourceGroupDeployment `
    -Name "Complete-$specificResource" `
    -Mode "Complete" `
    -ResourceGroupName $resourceGroupName `
    -TemplateFile "$PSScriptRoot\Application-templates\createApplication.json" `
    -SpecificResource "$specificResource" `
    -TemplateParameterFile "$PSScriptRoot\Application-templates\createApplication.parameters.$environment.json";
  } else {
    Write-Host "Deploying all resources (Complete mode)"
    New-AzResourceGroupDeployment `
    -Name "Complete-$specificResource" `
    -Mode "Complete" `
    -ResourceGroupName $resourceGroupName `
    -TemplateFile "$PSScriptRoot\Application-templates\createApplication.json" `
    -SpecificResource "$specificResource" `
    -TemplateParameterFile "$PSScriptRoot\Application-templates\createApplication.parameters.$environment.json";
  }
}
else {
  if ($test){
    Write-Host "Test deploying new resources (Incremental mode)"
    Test-AzResourceGroupDeployment `
    -Mode "Incremental" `
    -Name "Incremental-$specificResource" `
    -ResourceGroupName $resourceGroupName `
    -TemplateFile "$PSScriptRoot\Application-templates\createApplication.json" `
    -SpecificResource "$specificResource" `
    -TemplateParameterFile "$PSScriptRoot\Application-templates\createApplication.parameters.$environment.json";
  }else{
    Write-Host "Deploying new resources (Incremental mode)"
    New-AzResourceGroupDeployment `
    -Mode "Incremental" `
    -Name "Incremental-$specificResource" `
    -ResourceGroupName $resourceGroupName `
    -TemplateFile "$PSScriptRoot\Application-templates\createApplication.json" `
    -SpecificResource "$specificResource" `
    -TemplateParameterFile "$PSScriptRoot\Application-templates\createApplication.parameters.$environment.json";
  }
}
