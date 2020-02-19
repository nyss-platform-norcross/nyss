# A simple script that will:
# 1. Snapshot the blobs in the "to" container
# 2. Download the new blobs in the "from" container
# 3. Download the old blobs in the "to" container
# 4. Batch upload the downloaded blobs to the "to" container
param(
  [string] $from = "dev",
  [string] $to = "test"
)

$ScriptPath = Split-Path -parent $MyInvocation.MyCommand.Definition;
$folderForNewBlobs = "$ScriptPath\\tmp-new"
$folderForOldBlobs = "$ScriptPath\\tmp-old"

if (Get-Variable IsMacOS) {
  $folderForNewBlobs = "$ScriptPath/tmp-new"
  $folderForOldBlobs = "$ScriptPath/tmp-old"
}

Write-Host "1. Snapshotting blobs that exist in the destination $to..."
$blobs = az storage blob list --container-name nyss-blob-container --account-name nrxcbsnyssst$to | ConvertFrom-Json
$blobs | ForEach-Object {
  $snapResult = az storage blob snapshot --container-name nyss-blob-container --account-name nrxcbsnyssst$to --name $_.name | ConvertFrom-Json
  Write-Host "Snapshot of" $snapResult.name "last modified" $snapResult.properties.lastModified "created"
}

Write-Host "2. Download all blobs from source $from to $folderForNewBlobs"
Remove-Item $folderForNewBlobs -Recurse -ErrorAction Ignore
New-Item -Path $folderForNewBlobs -ItemType Directory
az storage blob download-batch --destination $folderForNewBlobs --source nyss-blob-container --account-name nrxcbsnyssst$from

Write-Host "3. Download old blobs from $to to $folderForOldBlobs"
Remove-Item $folderForOldBlobs -Recurse -ErrorAction Ignore
New-Item -Path $folderForOldBlobs -ItemType Directory
az storage blob download-batch --destination $folderForOldBlobs --source nyss-blob-container --account-name nrxcbsnyssst$to

Write-Host "4. Uploading downloaded blobs to $to..."
az storage blob upload-batch --source $folderForNewBlobs --destination nyss-blob-container --account-name nrxcbsnyssst$to