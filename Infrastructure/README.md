## Infrastructure (Azure resources)

### Application resources
Most resources are put into the same resource group per environment, except for the database. This way we can be a bit more sure to not delete any stored data we don't want to lose.

The following resources are per environment (`dev`, `test`, or `prod`):

| #  | Type                 | Name                                           | Remarks                                      |
| -- | -------------------- | ---------------------------------------------- | -------------------------------------------- |
| 1  | App Service          | `nrx-cbs-nyss-funcapp-<env>`                   |                                              |
| 2  | App Service          | `nrx-cbs-nyss-report-funcapp-<env>`            | Integrated with 'internal' subnet            |
| 3  | App Service          | `nrx-cbs-nyss-reportapi-webapp-<env>`          | Only available from 'internal' subnet        |
| 4  | App Service          | `nrx-cbs-nyss-webapp-<env>`                    |                                              |
| 5  | App Service (Slot)   | `staging (nrx-cbs-nyss-funcapp-<env>/staging)` |                                              |
| 6  | App Service (Slot)   | `staging (nrx-cbs-nyss-webapp-<env>/staging)`  |                                              |
| 7  | App Service plan     | `nrx-cbs-nyss-consumption-sp-<env>`            | Consumption plan                             |
| 8  | App Service plan     | `nrx-cbs-nyss-sp-<env>`                        | Standard plan                                |
| 9  | Application Insights | `nrx-cbs-nyss-funcapp-<env>`                   |                                              |
| 10 | Application Insights | `nrx-cbs-nyss-report-funcapp-<env>`            |                                              |
| 11 | Application Insights | `nrx-cbs-nyss-reportapi-webapp-<env>`          |                                              |
| 12 | Application Insights | `nrx-cbs-nyss-webapp-<env>`                    |                                              |
| 13 | Service Bus Namespace| `nrx-cbs-nyss-bus-<env>`                       |                                              |
| 14 | Service Bus Namespace| `nrx-cbs-nyss-bus-stg-<env>`                   |                                              |
| 15 | Storage account      | `nrxcbsnyssfuncappst<env>`                     | For function apps purpose                    |
| 16 | Storage account      | `nrxcbsnyssst<env>`                            | For general purpose                          |
| 17 | Virtual network      | `nrx-cbs-nyss-vnet-<env>`                      | With one 'internal' subnet                   |

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

#### Mandatory parameter:
* `-environment`: dev, test or production

#### Optional parameters:
* `-subscriptionId <string>`: Nice if you have been working with multiple subscription and needs to ensure that you are working on the right subscription
* `-complete`: If specified uses `complete` mode if not defaults to `incremental`
* `-test`: Test-runs the provisioning using the [Test-AzResourceGroupDeployment](https://docs.microsoft.com/en-us/powershell/module/az.resources/test-azresourcegroupdeployment?view=azps-3.3.0) command instead of `New-AzResourceGroupDeployment`
* `-specificResource <string>`: Can be used if you want to only provision one specific resource. **Be careful when using this in `complete` mode, it will delete the others!**

#### Examples
In a Powershell shell run:

```powershell
# For testing that the template is working:
.\provision-application-templates.ps1 -environment "<dev|test|demo|production>" -test
# For provisioning incrementally:
.\provision-application-templates.ps1 -environment "<dev|test|demo|production>"
# ..or to do a complete deployment:
.\provision-application-templates.ps1 -environment "<dev|test|demo|production>" -complete
```

If you want to only deploy a specific resource:
```powershell
.\provision-application-templates.ps1 -environment dev -specificResource "nrx-cbs-nyss-webapp-dev"
```

For provisioning the DB resources:
```powershell
# For testing that the template is working
.\provision-db-templates.ps1 -environment "<dev|production>" -test
# For incremental
.\provision-db-templates.ps1 -environment "<dev|production>"
# ..or complete
.\provision-db-templates.ps1 -environment "<dev|production>" -complete
```
