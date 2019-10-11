This console application is used for migrating the database during deployment, instead of running the migration on startup of the web app. It takes the connection string as an input parameter and then creates and migrates the given database with `NyssContext`. 

Example usage:

```cmd
dotnet run RX.Nyss.Data.MigrationApp.dll "<your-db-connection-string-here>"

rem ..or if you have built the project as a self-contained exe:
RX.Nyss.Data.MigrationApp.exe "<your-db-connection-string-here>"
```
