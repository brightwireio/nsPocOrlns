# nsPocOrlns
POC to stream Event Hub to Orleans in-proc

## Steps to Setup

### Azure Resources
1. Create an Azure resource group to house all the resources to follow.
1. Create an Azure SQL DB.
2. Create an Azure Storage Account (standard general-purpose V2). Also create a blob container called tripdata within the account.
3. Create an Azure Event Hub (Premium for testing high load or standard for basic).
4. Create an Azure Function App (Premium to test higher load or consumption for basic).
5. Create an Azure App Service Plan (I used Linix, P2V3 for demo, but any PRODUCTION SKU will work. I used a single server in the demo, but can scale out)
6. Create an Azure VNET with SUBNET (address space 10.0.0.0/24)
7. Create an Azure Web App in the App Service Plan from (5) and assign to the VNET and SUBNET from 6.

### Configure Services
1. Edit local.settings.json in the nsPocOrlns.TrafficSimulator project to use connection settings from the resources configured above. (for testing locally).
2. Edit appsettings.json in the nsPocOrlns.WebHost3 project to use connection settings from the resources configured above. (for testing locally). 
3. Deploy nsPocOrlns.TrafficSimulator to the Function App created in (4) above.
4. Edit the Function App Config settings on Azure to relect those if the resources created above.
5. Deploy nsPocOrlns.WebHost3 to the Web App created in (7) above.
6. Edit theWeb App Config settings on Azure to relect those if the resources created above.

### Run Some Tests
1. Ensure the Function App and Web App are up and running on Azure.
2. Get the Http Trigger Url for the SimulatorHttp function from the function overview page in the Azure portal.
3. Use the URL from (2) in POSTMAN (or the like) and pass the followig query parameters:
    1. totalUnits - the total number of units to use in the simulation.
    2. batchSize - the size of the batches to start the units in. If totalUnits and batchSize are the same, all will start in one go.
    3. maxTripLength - the max number of location changed events to use for a trip.
    4. minTripLength - the min number of location changed events to use for a trip.
4. Fire of the request, and monitor the function app and web app in Azure.
5. View the resulting trips in Azure SQL, check location events in the tripdata container.
6. Get the current location of a Unuit by calling the /unit/{unitId} endpoint on the Web API.
7. Get a list of trips by calling /unit/{UnitId}/trips.
8. Get a list of events for a trip by calling /unit/{unitId}/trips/{tripId}/events.
9. You can also modify the simulator to push event data into SQL Server and the call SimulatorHttpV2 on the Function app to create a high load test.




    


 
