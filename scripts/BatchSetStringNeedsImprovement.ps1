# A simple script for use after adding a new language, that will:
# 1. Snapshot the blobs in the dev container
# 2. Download all blobs in the dev container
# 3. Set the needsImprovement value to true for all strings in the strings blobs
# 4. Batch upload the blob to dev

$ScriptPath = Split-Path -parent $MyInvocation.MyCommand.Definition;
$folderForNewBlobs = Join-Path -Path $ScriptPath -ChildPath "tmp-new"
$folderForOldBlobs = Join-Path -Path $ScriptPath -ChildPath "tmp-old"
$pathToOldStringsBlob = Join-Path -Path $folderForOldBlobs -ChildPath "stringsResources.json"
$pathToOldEmailStringsBlob = Join-Path -Path $folderForOldBlobs -ChildPath "emailContentResources.json"
$pathToOldSmsStringsBlob = Join-Path -Path $folderForOldBlobs -ChildPath "smsContentResources.json"
$pathToNewStringsBlob = Join-Path -Path $folderForNewBlobs -ChildPath "stringsResources.json"
$pathToNewEmailStringsBlob = Join-Path -Path $folderForNewBlobs -ChildPath "emailContentResources.json"
$pathToNewSmsStringsBlob = Join-Path -Path $folderForNewBlobs -ChildPath "smsContentResources.json"

Write-Host "1. Snapshotting blobs that exist in the destination dev …"
$blobs = az storage blob list --container-name nyss-blob-container --account-name nrxcbsnyssstdev | ConvertFrom-Json
$blobs | ForEach-Object {
  $snapResult = az storage blob snapshot --container-name nyss-blob-container --account-name nrxcbsnyssstdev --name $_.name | ConvertFrom-Json
  Write-Host "Snapshot of" $snapResult.name "last modified" $snapResult.properties.lastModified "created"
}

Write-Host "2. Download all blobs from source dev to $folderForNewBlobs …"
Remove-Item $folderForNewBlobs -Recurse -ErrorAction Ignore
New-Item -Path $folderForNewBlobs -ItemType Directory
az storage blob download-batch --destination $folderForNewBlobs --source nyss-blob-container --account-name nrxcbsnyssstdev

Write-Host "2. Download old blobs from dev to $folderForOldBlobs …"
Remove-Item $folderForOldBlobs -Recurse -ErrorAction Ignore
New-Item -Path $folderForOldBlobs -ItemType Directory
az storage blob download-batch --destination $folderForOldBlobs --source nyss-blob-container --account-name nrxcbsnyssstdev

Write-Host "3. Set the needsImprovement property to true for all strings in strings blob …"
$stringsBlob = Get-Content $pathToOldStringsBlob -raw | ConvertFrom-Json
$stringsBlob.strings | ForEach-Object {
  if($_.PSobject.Properties.Name -contains "needsImprovement"){
    $_.needsImprovement=$true
  } else {
    $_ | Add-Member -Name "needsImprovement" -Value $true -MemberType NoteProperty
  }
}
$stringsBlob | ConvertTo-Json -depth 32| set-content $pathToNewStringsBlob

Write-Host "3. Set the needsImprovement property to true for all strings in email blob …"
$emailStringsBlob = Get-Content $pathToOldEmailStringsBlob -raw | ConvertFrom-Json
$emailStringsBlob.strings | ForEach-Object {
  if($_.PSobject.Properties.Name -contains "needsImprovement"){
    $_.needsImprovement=$true
  } else {
    $_ | Add-Member -Name "needsImprovement" -Value $true -MemberType NoteProperty
  }
}
$emailStringsBlob | ConvertTo-Json -depth 32| set-content $pathToNewEmailStringsBlob

Write-Host "3. Set the needsImprovement property to true for all strings in sms blob …"
$smsStringsBlob = Get-Content $pathToOldSmsStringsBlob -raw | ConvertFrom-Json
$smsStringsBlob.strings | ForEach-Object {
  if($_.PSobject.Properties.Name -contains "needsImprovement"){
    $_.needsImprovement=$true
  } else {
    $_ | Add-Member -Name "needsImprovement" -Value $true -MemberType NoteProperty
  }
}
$smsStringsBlob | ConvertTo-Json -depth 32| set-content $pathToNewSmsStringsBlob

Write-Host "4. Uploading downloaded blobs to dev …"
az storage blob upload-batch --source $folderForNewBlobs --destination nyss-blob-container --account-name nrxcbsnyssstdev