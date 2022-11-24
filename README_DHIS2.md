# DHIS2 Integration

## Config

Nyss app includes the integration with DHIS2 API. If you want to test or develop that solution you can connect
to the existing DHIS2 instance or host such an API on your local machine.

Here is the tutorial:
https://dhis2trainingland.com/eportal/?tutorial=dhis2-installation-on-windows-local-server

Or alternatively you can run docker image featuring everything you need to host the app. Just run `docker-compose up` with the following docker-compose.yml.
Don't forget to have dhis.conf file in the proper volume path.

docker-compose.yml
```yaml
version: '3'
services:
  db:
    image: mdillon/postgis:10-alpine
    command: postgres -c max_locks_per_transaction=100
    environment:
      POSTGRES_USER: dhis
      POSTGRES_DB: dhis2
      POSTGRES_PASSWORD: dhis
    ports:
      - "5432:5432"
    volumes:
      - ./data:/var/lib/postgresql/data

  web:
    image: dhis2/core:2.33.0
    volumes:
      - ./config/dhis2_home/dhis.conf:/DHIS2_home/dhis.conf
    ports:
      - "8080:8080"
    depends_on:
      - db
```
dhis.conf
```
connection.dialect = org.hibernate.dialect.PostgreSQLDialect
connection.driver_class = org.postgresql.Driver
connection.url = jdbc:postgresql://db/dhis2
connection.username = dhis
connection.password = dhis
```

## Instance preparation

Enter DHIS2 app and add new entities via "Maintenance" module:

* add data element (Tracker type)
* add organisation unit
* add program

In the specific program's page:

* in "Assign data elements" tab assign created earlier data element
* in "Access" tab (Organisation units) add created earlier organisation unit (start typing to see list of the organisations)
* in "Access" tab (Roles and access) give to your user "Can capture and view" permission

## Usage

Nyss app at some point will perform POST request similar to that below. 

```json
{
    "program": "YCpwWJS5QHo",
    "orgUnit": "mrag4rqqOSm",
    "eventDate": "2022-11-17",
    "dataValues": [
        {
            "dataElement": "C7dNTNBjQq3",
            "value": "female"
        }
    ]
}

```

You can observe registered events in "Capture" module.

See DHIS2 documentation for the overview of the API and Nyss data contracts for the required data elements.