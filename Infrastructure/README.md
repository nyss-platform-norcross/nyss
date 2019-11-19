## Infrastructure (Azure resources)

### Application resources
Most resources are put into the same resource group per environment, except for the database. This way we can be a bit more sure to not delete any stored data we don't want to lose.

The following resources are per environment (`dev`, `test`, or `production`):

| Type                          | Name                                          | Remarks                               |
| ---                           | ---                                           | ---                                   |
| Resource group                | `nrx-cbs-<env>-rg`                            |                                       |
| Virtual network               | `nrx-cbs-<env>-nyss-vnet`                     | With one 'internal' subnet            |
| Service Plan                  | `nrx-cbs-<env>-nyss-webapp-sp`                | Standard plan                         |
| Service Plan                  | `nrx-cbs-<env>-nyss-webapp-consumption-sp`    | Consumption plan                      |
| App Service                   | `nrx-cbs-<env>-nyss-webapp`                   | TODO: Slots config                    |
| App Service                   | `nrx-cbs-<env>-nyss-reportapi-webapp`         | Only from 'internal' subnet           |
| Function App                  | `nrx-cbs-<env>-nyss-report-funcapp`           | Integrated with 'internal' subnet     |
| Function App                  | `nrx-cbs-<env>-nyss-funcapp`                  |                                       |
| AppInsights                   | `nrx-cbs-<env>-nyss-report-funcapp`           |                                       |
| AppInsights                   | `nrx-cbs-<env>-nyss-webapp`                   |                                       |
| AppInsights                   | `nrx-cbs-<env>-nyss-reportapi-webapp`         |                                       |
| AppInsights                   | `nrx-cbs-<env>-nyss-funcapp`                  |                                       |
| ServiceBus Namespace          | `nrx-cbs-<env>-nyss-bus`                      |                                       |
| Storage account               | `nrxcbs<env>nyssfuncappst`                    | For function apps                     |
| Storage account               | `nrxcbs<env>nyssst`                           | For general purpose                   |

Here's a diagram that shows their dependencies and the naming for dev env applied:

![arm-template](./arm-resources.png)

### Database resources

The database resources consists of two Database servers (one for staging envs and one for production) and one Database per environment.

| Type                      | Name                                  | Remarks                               |
| ---                       | ---                                   | ---                                   |
| Azure SQL Server          | `nrx-cbs-<env>-nyss-sqlserver`        | In `nrx-cbsdb-<dev|production>-rg`    |
| Azure SQL DB              | `nrx-cbs-<env>-nyss-sqldb`            | Both dev and test db resides in the `nrx-cbsdb-dev-rg` and in the same SQL server    |

### Provision infrastructure in Azure

> **⚠ NB!**: Take care when provisioning infrastructure, especially if you are doing a `complete` deploy!

From a Powershell shell run:

```powershell
# For provisioning most resources:
.\Infrastructure\provision-application-templates.ps1 -environment "<env>"
# ..or to do a complete deployment:
.\Infrastructure\provision-application-templates.ps1 -environment "<env>" -complete

# For provisioning the DB resources:
.\Infrastructure\provision-db-templates.ps1 -environment "<env>"
# ..or
.\Infrastructure\provision-db-templates.ps1 -environment "<env>" -complete
```
