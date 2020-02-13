# Nyss

<img src="src\RX.Nyss.Web\ClientApp\public\images\logo.svg" alt="The Nyss logo" width="200"/>

### Welcome to the repository for the new community-based surveillance solution, called *Nyss*!

Nyss is a Norwegian word and means to "get the wind of something". The first Norwegian computer was called [Nusse](https://no.wikipedia.org/wiki/Nusse).

Nyss is a reimplementation of [the previous CBS solution](https://github.com/IFRCGo/cbs) and proudly extending all the great work done there.

## Getting started

### Prerequisites

* [.NET Core Installer - SDK 3.1.100](https://dotnet.microsoft.com/download/dotnet-core/3.1)
* [Visual Studio 2019](https://visualstudio.microsoft.com/pl/downloads/)
* [Visual Studio Code](https://code.visualstudio.com/Download)
* [Microsoft Azure Storage Explorer](https://azure.microsoft.com/pl-pl/features/storage-explorer/)
* ...or if you are using mac/linux: [Azurite](https://github.com/azure/azurite)

### Recommended plugins and extensions

* [ReSharper](https://www.jetbrains.com/resharper/download/)
* [C# for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)
* [GitLens](https://marketplace.visualstudio.com/items?itemName=eamodio.gitlens)

### How to run the web application (_RX.Nyss.Web_) locally

1. Open command prompt (`cmd`) in the root of the application repository.
2. Navigate to the web application directory:
   1. `cd src/RX.Nyss.Web`
3. Run database migrations:
   1. Make sure that a connection string for _NyssDatabase_ set in `appsettings.Development.json` is correct. By default, [SQL Server Express LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) instance is used.
   2. `dotnet ef database update --context NyssContext`
   3. `dotnet ef database update --context ApplicationDbContext`\
  See [Database and migrations](src/RX.Nyss.Data/README.MD) for further details on working with the database.
4. Set up Blob Object connection string
   1. Make sure that a shared access signature for _SmsGatewayBlobContainer_ set in `appsettings.Development.json` is correct. You can generate a new one by opening Microsoft Azure Storage Explorer, then clicking right mouse button on _Local & Attached &rarr; Storage Accounts &rarr; (Emulator - Default Ports)(Key) &rarr; Blob Containers &rarr; sms-gateway_ node and selecting _Get Shared Access Signature..._ In a pop-up window ensure that the _Expiry time_ is set far in the future enough and _Write_ checkbox on the _Permissions_ list is selected. The _SmsGatewayBlobContainer_ should have the following format: `BlobEndpoint=https://{Environment URL}/sms-gateway;SharedAccessSignature={Query string without question mark at the beginning}`.
5. Start the application:
   1. `dotnet run` - the first build may take up to 5 minutes, the applicatiion is ready to open when you see _Compiled successfully!_ in the _npm_ command prompt.
   2. Open page [https://localhost:5001/](https://localhost:5001/).
   3. A default login of System Administrator is "admin@domain.com" and a password is "P@ssw0rd".

### How to run the function app (_RX.Nyss.FuncApp_) locally

1. Set _SERVICEBUS_CONNECTIONSTRING_, _SERVICEBUS_REPORTQUEUE_, _MailjetApiKey_, _MailjetApiSecret_ and _MailjetFromAddress_ in your `local.settings.json` or in the user secrets (`secrets.json`) file. 
    * The user secrets file is the same for the FuncApp as for the WebApp. 
    * To access it: In VS2019, right-clicking either the FuncApp or WebApp project, and click "Manage User Secrets". If you are using Linux/MacOs, it should be located here: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json` (the _user_secrets_id_ can be found in the .csproj files)
2. Open Microsoft Azure Storage Explorer.
3. Add a new blob container called "sms-gateway" to the Local Storage Account Emulator. If you are not running on Windows, you need to use another emulator called [Azurite](https://github.com/azure/azurite).
4. Create a new text file `authorized-api-keys.txt` with a content "api-key" and upload it to _sms-gateway_ container.
4. If you want to test sending emails locally, create a new text file `whitelisted-email-addresses` with a list of email addresses that you want to use (separated by newline) and upload it to the _sms-gateway_ container.
5. Open Visual Studio.
6. In _Solution Explorer_ window, set _RX.Nyss.FuncApp_ as a startup project.
7. Debug &rarr; Start Debugging.

References: [Code and test Azure Functions locally](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-local)

## How to contribute

### Code review checklist

* PR branch is on top of _master_
* Build and all unit tests are green (and business logic is covered by unit tests)
* The code fulfills the acceptance criteria
* The code is easy to understand or has substantial documentation on how to understand
* "How to test" steps should be applied and understandable for both tester and reviewer
* Test that it is working in dev once the PR is completed, merged to master and deployed
* The code is formatted according to code conventions
* If data model changes, ER diagram needs to be updated

### Documentation

* How to run locally
  * Tools and frameworks needed
  * Local configuration
  * Useful commands
* High-level architecture diagram
* Data model ER diagram
* The code should be self-explanatory, but when not it should be possible for the developer looking at to get some help by comments.
* Swagger: xmldoc should be used for specifying endpoints in API controllers.

### Code conventions

* C# code style should be specified in the [.editorconfig](./.editorconfig) file in the repository root directory. Examples:
  * line length (and wrap)
  * public members on top
  * object initializers
  * usings
* Keep it simple

### Git commit message style

* [Git Commit Best Practices](https://github.com/trein/dev-best-practices/wiki/Git-Commit-Best-Practices)
* Commit often and push if you feel for it. Squash them if a lot of them doesn't add value
* Branch name format: `feature or bugfix/workitem-number`-`workitem-name`, eg:
  * `feature/23-add-system-administrator`
  * `bugfix/73-add-missing-margins-in-project-dashboard`
* Commit messages: Imperative style: Write what the code now does and not what the developer has done. Example:

```
# wrong
Added mapper, updated controller

# correct
Add validation rules for creating a supervisor
- Add mapper
- Update supervisor controller
```
