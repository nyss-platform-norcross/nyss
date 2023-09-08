<img src="src\RX.Nyss.Web\ClientApp\public\images\logo.svg" alt="The Nyss logo" width="200"/>

### Welcome to the repository for the new community-based surveillance solution, called _Nyss_!

Nyss is a Norwegian word and means to "get the wind of something". The first Norwegian computer was called [Nusse](https://no.wikipedia.org/wiki/Nusse).

Nyss is a reimplementation of [the previous CBS solution](https://github.com/IFRCGo/cbs) and proudly extending all the great work done there.

## Getting started

How to run and develop everything on your local machine.

### Prerequisites

- [.NET Core Installer - SDK 3.1.100](https://dotnet.microsoft.com/download/dotnet-core/3.1)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [Microsoft Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator). Alternatively, especially if you are using mac/linux: [Azurite](https://github.com/azure/azurite)
- [Microsoft Azure Storage Explorer](https://azure.microsoft.com/pl-pl/features/storage-explorer/)

### Mac Prerequisites (M1/M2)

- [.NET Core Installer - SDK 7.0.10](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [Homebrew](https://brew.sh/)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [Docker](https://docs.docker.com/desktop/install/mac-install/) After downloading: Go to Settings -> Features in development and enable Use Rosetta for x86/amd64 emulation on Apple Silicon
- Azurite

  - Downlad as a docker image:
    ```
    docker run --name azurite -d --restart unless-stopped -p 10000:10000 -p 10001:10001 -p 10002:10002 -v /Users/<your username>/azurite/data  mcr.microsoft.com/azure-storage/azurite
    ```

- [Microsoft Azure Storage Explorer](https://azure.microsoft.com/en-us/products/storage/storage-explorer)
- [Azure Data Studio](https://docs.microsoft.com/en-us/sql/azure-data-studio/download-azure-data-studio?view=sql-server-ver15)

### Recommended tools

- IDE:
  - [Visual Studio 2019](https://visualstudio.microsoft.com/pl/downloads/) (and [ReSharper](https://www.jetbrains.com/resharper/download/))
  - [Rider](https://www.jetbrains.com/rider/)
  - [Visual Studio Code](https://code.visualstudio.com/Download)
- Git client:
  - [TortoiseGit](https://tortoisegit.org/)(Windows only)
  - [Fork](https://git-fork.com/)
  - [GitLens](https://marketplace.visualstudio.com/items?itemName=eamodio.gitlens) (Extension for VS code)
- [Azure Data Studio](https://docs.microsoft.com/en-us/sql/azure-data-studio/download-azure-data-studio?view=sql-server-ver15) or [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver15)
- [Service Bus Explorer](https://github.com/paolosalvatori/ServiceBusExplorer/releases) (Windows only)

### Run database migrations

- Make sure the connection string for `NyssDatabase` in `appsettings.Development.json` is correct. By default, a [SQL Server Express LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) instance is used, which should work out of the box if you are using Visual Studio on Windows. On mac, an alternative can be to run SQL Server in in a docker container. ([A nice guide](https://getadigital.com/blog/setting-up-sql-server-on-docker-in-mac-os/))
- Ensure you have the ef core dotnet tool installed:

```
  dotnet tool install --global dotnet-ef
```

- Update the two data base contextes:

```
  dotnet ef database update --context NyssContext
  dotnet ef database update --context ApplicationDbContext
```

See [Database and migrations](src/RX.Nyss.Data/README.MD) for further details on working with the database.

### Run database migrations on Mac with Docker (M1/M2)

Apple has released its own chip which uses ARM architecture and causes a problem since SQL Server isn’t supported on the ARM architecture. However, the container image for Azure SQL Edge can be installed instead of the SQL Server image.

- Open the terminal and run:

- In you terminal, run:

```
  docker pull mcr.microsoft.com/mssql/server:2022-latest
```

- In the follwing command, you will need a password that complies to [these guidelines](https://learn.microsoft.com/en-gb/sql/relational-databases/security/password-policy?view=sql-server-ver16). The following command generates a SQL server with a default system administrator (`sa`) user with the password that you will set for `MSSQL_SA_PASSWORD`.

```
  sudo docker run --cap-add SYS_PTRACE -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=YourStrong@Passw0rd' -p 1433:1433 --platform linux/amd64 --name sqledge -d mcr.microsoft.com/mssql/server:2022-latest
```

- In `src/RX.Nyss.Web/appsettings.Development.json`, change the string value of ConnectionStrings.NyssDatabase to:

```
  "Server=localhost,1433\\;Database=Rx.Nyss.Db;User=sa;Password=YourStrong@Passw0rd;"
```

- You can now connect to your Azure SQL Edge installation by using the details provided when launching the Docker image. It is recommended to connect to Azure Data Studio. Below is a screenshot of how to connect via Azure Data Studio. Use all the credentials from the image:

<img src="data_storage.png" width="400">

- Ensure you have the ef core dotnet tool installed:

```
  dotnet tool install --global dotnet-ef
```

- Navigate to the `src/RX.Nyss.Web` folder and update the two data base contextes:

```
  dotnet ef database update --context NyssContext
  dotnet ef database update --context ApplicationDbContext
```

### Set up Blob storage

- Make sure that the Azure storage emulator (or Azurite for Mac) is running, and open the emulator connection in Azure Storage Explorer.

  - Connect to Azure resources
  - Select Local storage emulator
    - Display name can be changed
    - Use default settings on everything

- Create the containers that are needed in `Storage Accounts / (Emulator - Default Ports) (Key)` (all containers are listed in the AppSettings.json file in the Nyss.Web project):
  - `sms-gateway`
  - `nyss-blob-container`
  - `nyss-agreements-container`
- Inside each container, you will need these files. You may need to contact your supervisor for these.

  - Inside `sms-gateway`:

    - `authorized-api-keys.txt`

  - Inside `nyss-blob-container`:

    - `emailContentResources.json`
    - `smsContentResources.json`
    - `stringsResources.json`
    - `platform-agreement-en.pdf`
    - `platform-agreement-es.pdf`
    - `platform-agreement-fr.pdf`

  - Inside `nyss-agreements-container`:

    - Not specified

- If you want to test sending emails locally, create a new text file `whitelisted-email-addresses.txt` with a list of email addresses that you want to use (separated by newline) and upload it to the `sms-gateway` container.
- If you want to test sending SMSes locally, create a new text file `whitelisted-phone-numbers.txt` with a list of email addresses that you want to use (separated by newline) and upload it to the `sms-gateway` container.

### Add UserSecrets

In order to test everything locally, there are a couple of sensitive configuration variables that should be set. We store these as UserSecrets in order make them less prone to being checked in to our git repository. Most should work without these, but some parts of nyss uses some services that you can't run locally and therefore need a connection string, api key of some sort. You can find all keys values in the `appsettings.json` files. The most important ones to think of is:

- `SERVICEBUS_CONNECTIONSTRING`/`ConnectionStrings.ServiceBus` (in order to send service bus messages locally)
- `MailConfig.Sendgrid.ApiKey` (if you are going to test sending emails locally)

The user secrets file is the same for all running applications. How to access it:

- In Visual Studio Code, the extension with the id: `doggy8088.netcore-extension-pack` can be used to manage the user secret.
  - After downloading the extension, open `RX.Nyss.Web` in your VSCode explorer and right-click `RX.Nyss.Web.csproj` and press `Manage User Secrets`.
  - A `secrets.json` file will appear. Contact your supervisor for the content inside this file.
- In Rider, right click the `RX.Nyss.Web` folder and hover over `tools` and then select `.NET User Secrets`.
  - A `secrets.json` file will appear. Contact your supervisor for the content inside this file.
- In VS2019, right-click either a FunctionApp or WebApp project, and click "Manage User Secrets".
- On Windows this file is normally located at `%AppData%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
- On Linux/MacOs, it should be located here: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json` (the `user_secrets_id` can be found in the .csproj files)

### Run the web applications (`RX.Nyss.Web` and `RX.Nyss.ReportApi`)

- Naviagte to `src/RX.Nyss.Web` or `src/RX.Nyss.ReportApi` directory and in a terminal run `dotnet run`. The first build may take up to 5 minutes, the applicatiion is ready to open when you see _Compiled Successfully!_ in the _npm_ command prompt.
- Each application should open a browser window, or you can manually open [https://localhost:5001/](https://localhost:5001/) (Web application) or [https://localhost:5003/swagger](https://localhost:5003/swagger) (Report Api).
- A default login of System Administrator is `admin@domain.com` and a password is `P@ssw0rd`.

### Run the function apps (`RX.Nyss.FuncApp` and `RX.Nyss.ReportFuncApp`)

- Naviagte to `src/RX.Nyss.FuncApp` or `src/RX.Nyss.ReportFuncApp` directory and in a terminal run `func host start`.
  References: [Code and test Azure Functions locally](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-local)

### DHIS2 Integration

Nyss app includes the integration with DHIS2 API. If you want to test or develop that solution, please visit that readme file to
setup your machine.

[DHIS2 integration instruction](./README_DHIS2.md)

## And you are done!

## Some contribution notes

### Code review checklist

- PR branch is on top of _master_
- Build and all unit tests are green (and business logic is covered by unit tests)
- The code fulfills the acceptance criteria
- The code is easy to understand or has substantial documentation on how to understand
- "How to test" steps should be applied and understandable for both tester and reviewer
- Test that it is working in dev once the PR is completed, merged to master and deployed
- The code is formatted according to code conventions
- If data model changes, ER diagram needs to be updated

### Code conventions

- C# code style should be specified in the [.editorconfig](./.editorconfig) file in the repository root directory. Examples:
  - line length (and wrap)
  - public members on top
  - object initializers
  - usings

### Git commit message style

- [Git Commit Best Practices](https://github.com/trein/dev-best-practices/wiki/Git-Commit-Best-Practices)
- Commit often and push if you feel for it. Squash them if a lot of them doesn't add value
- Branch name format: `feature or bugfix/workitem-number`-`workitem-name`, eg:
  - `feature/23-add-system-administrator`
  - `bugfix/73-add-missing-margins-in-project-dashboard`
- Commit messages: Imperative style: Write what the code now does and not what the developer has done. Example:

```

# wrong

Added mapper, updated controller

# correct

Add validation rules for creating a supervisor

- Add mapper
- Update supervisor controller

```

```

```
