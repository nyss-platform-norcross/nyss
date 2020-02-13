# A simple script that will:
# 1. Snapshot the blobs in the "to" container
# 2. Download the blobs in the "from" container
# 3. Batch upload the downloaded blobs to the "to" container
param(
  [string] $from = "dev",
  [string] $to = "test"
)

$ScriptPath = Split-Path -parent $MyInvocation.MyCommand.Definition;
$folderForBlobs = Join-Path $ScriptPath "/tmp" -Resolve

Write-Host "1. Snapshotting blobs that exist in the destination $to..."
$blobs = az storage blob list --container-name nyss-blob-container --account-name nrxcbsnyssst$to | ConvertFrom-Json
$blobs | ForEach-Object {
  $snapResult = az storage blob snapshot --container-name nyss-blob-container --account-name nrxcbsnyssst$to --name $_.name | ConvertFrom-Json
  Write-Host "Snapshot of" $snapResult.name "last modified" $snapResult.properties.lastModified "created"
}

Write-Host "2. Download all blobs from source $from to $folderForBlobs"
Remove-Item $folderForBlobs -Recurse -ErrorAction Ignore
New-Item -Path $folderForBlobs -ItemType Directory
az storage blob download-batch --destination $folderForBlobs --source nyss-blob-container --account-name nrxcbsnyssst$from

Write-Host "3. Uploading downloaded blobs to $to..."
az storage blob upload-batch --source $folderForBlobs --destination nyss-blob-container --account-name nrxcbsnyssst$to