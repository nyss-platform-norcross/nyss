This project contains the `NyssContext` database model and its migrations. An updated ER diagram of the model can be found [here](./nyss-er-diagram.png).


## Migrations
If you need to change the data model, you would need to create a new migration. A migration contains the sql scripts needed to alter the database schema to correspond with the model.

If you are using Visual Studio you can use the Package Manager Console and the Entity Framework Core tools there, or you can use the .NET cli version. The cli version will be used in the examples below. That one can be installed like this:

```cli
dotnet tool install --global dotnet-ef
```

[Entity Framework Core tools reference](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet)

So if you have made any changes to the existing model or created/deleted any classes in this project and want to create a migration, run the following command:

```cmd
dotnet ef migrations add "YourDescriptionOfTheMigration" --startup-project ..\RX.Nyss.Web\RX.Nyss.Web.csproj --context NyssContext
```

This will then create a .cs file in the `./Migrations` folder with two methods: `Up` and `Down`. These methods contains what would be applied to the database. It can be a good idea to check that this looks correct before proceeding with the actual update of the database. But if everything looks fine the database can be updated by using the following command:

```cmd
dotnet ef database update --startup-project ..\RX.Nyss.Web\RX.Nyss.Web.csproj --context NyssContext
```

If not specified, `dotnet ef database update` command updates to the latest migration, but you can also specify which version you want to update to, either using the name or Id. If you specify "0", it will update the databse back to how it was before the initial migration.