Write-Host "Updating database..."
dotnet ef database update -s ..\RX.Nyss.Web\RX.Nyss.Web.csproj --context=NyssContext
