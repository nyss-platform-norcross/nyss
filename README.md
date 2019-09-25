# RX.Nyss

Welcome to the repository for the new CBS solution, called *NYSS*! 

Nyss is a norwegian word and means to "get the wind of something". And the first norwegian computer was called [Nusse](https://no.wikipedia.org/wiki/Nusse).

## Getting started
dotnet run

## Infrastructure (Azure resources)
Most resources are put into the same resource group per environment, except for the database. This way we can be a bit more sure to not delete any stored data we don't want to lose.

The following resources are per environment (`dev`, `test`, or `production`):

| Type                      | Name                                  | Remarks                               |
| ---                       | ---                                   | ---                                   |
| Resource group            | `nrx-cbs-<env>-rg`                    |                                       |
| App Service               | `nrx-cbs-<env>-nyss-webapp`           | Slots TBD                             |
| AppInsights               | `nrx-cbs-<env>-nyss-webapp`           |                                       |
| App Service Plan          | `nrx-cbs-<env>-nyss-webapp-sp`        |                                       |
| Function App              | `nrx-cbs-<env>-nyss-func`             | TBD                                   |
| ServiceBus Namespace      | `nrx-cbs-<env>-nyss-bus`              |                                       |

The database resources consists of two Database servers (one for staging envs and one for production) and one Database per environment.

| Type                      | Name                                  | Remarks                               |
| ---                       | ---                                   | ---                                   |
| Azure SQL Server          | `nrx-cbs-<env>-nyss-sqlserver`        | In `nrx-cbsdb-<dev|production>-rg`    |
| Azure SQL                 | `nrx-cbs-<env>-nyss-sqldb`            | In `nrx-cbsdb-<dev|production>-rg`    |

### Provision infrastructure in Azure

From a Powershell shell run:

```powershell
.\Infrastructure\infrastructure.ps1 -environment "<env>"

# ..or to do a complete deployment:
.\Infrastructure\infrastructure.ps1 -environment "<env>" -complete

```
