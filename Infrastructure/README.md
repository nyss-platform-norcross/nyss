## Infrastructure (Azure resources)

### Application resources
Most resources are put into the same resource group per environment, except for the database. This way we can be a bit more sure to not delete any stored data we don't want to lose.

The following resources are per environment (`dev`, `test`, or `prod`):

| Type                 | Name                                       | Remarks                           |
| -------------------- | ------------------------------------------ | --------------------------------- |
| Resource group       | `nrx-cbs-rg-<env>`                         |                                   |
| Virtual network      | `nrx-cbs-nyss-vnet-<env>`                  | With one 'internal' subnet        |
| Service Plan         | `nrx-cbs-nyss-webapp-sp-<env>`             | Standard plan                     |
| Service Plan         | `nrx-cbs-nyss-webapp-consumption-sp-<env>` | Consumption plan                  |
| App Service          | `nrx-cbs-nyss-webapp-<env>`                | TODO: Slots config                |
| App Service          | `nrx-cbs-nyss-reportapi-webapp-<env>`      | Only from 'internal' subnet       |
| Function App         | `nrx-cbs-nyss-report-funcapp-<env>`        | Integrated with 'internal' subnet |
| Function App         | `nrx-cbs-nyss-funcapp-<env>`               |                                   |
| AppInsights          | `nrx-cbs-nyss-report-funcapp-<env>`        |                                   |
| AppInsights          | `nrx-cbs-nyss-webapp-<env>`                |                                   |
| AppInsights          | `nrx-cbs-nyss-reportapi-webapp-<env>`      |                                   |
| AppInsights          | `nrx-cbs-nyss-funcapp-<env>`               |                                   |
| ServiceBus Namespace | `nrx-cbs-nyss-bus-<env>`                   |                                   |
| Storage account      | `nrxcbsnyssfuncappst<env>`                 | For function apps                 |
| Storage account      | `nrxcbsnyssst<env>`                        | For general purpose               |

Here's a diagram that shows their dependencies and the naming for dev env applied:

![arm-template](./arm-resources.png)

### Database resources

The database resources consists of two Database servers (one for staging envs and one for production) and one Database per environment.

| Type             | Name                           | Remarks                                                                           |
| ---------------- | ------------------------------ | --------------------------------------------------------------------------------- |
| Azure SQL Server | `nrx-cbs-nyss-sqlserver-<env>` | In `nrx-cbsdb-<dev|production>-rg`                                                |
| Azure SQL DB     | `nrx-cbs-nyss-sqldb-<env>`     | Both dev and test db resides in the `nrx-cbsdb-dev-rg` and in the same SQL server |

### Provision infrastructure in Azure

> **⚠ NB!**: Take care when provisioning infrastructure, especially if you are doing a `complete` deploy!

From a Powershell shell run:

```powershell
# For provisioning most resources:
.\Infrastructure\provision-application-templates.ps1 -environment "<dev|test|production>"
# ..or to do a complete deployment:
.\Infrastructure\provision-application-templates.ps1 -environment "<dev|test|production>" -complete

# For provisioning the DB resources:
.\Infrastructure\provision-db-templates.ps1 -environment "<dev|production>"
# ..or
.\Infrastructure\provision-db-templates.ps1 -environment "<dev|production>" -complete
```
