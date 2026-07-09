$migrationName = $args[0]

if (!$migrationName)
{
  Write-Error "Please specify migration name"
  exit
}

Write-Host "Adding migration '$migrationName'..."
dotnet ef migrations add $migrationName -s ..\RX.Nyss.Web\RX.Nyss.Web.csproj --context=NyssContext
