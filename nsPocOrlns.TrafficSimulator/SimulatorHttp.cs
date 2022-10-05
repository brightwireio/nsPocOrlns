//namespace nsPocOrlns.TrafficSimulator;

//public static class SimulatorHttp
//{
//    [FunctionName("SimulatorHttp")]
//    public static async Task<IActionResult> Run(
//        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,ILogger log,[DurableClient] IDurableEntityClient client)
//    {
//        var totalUnits = int.Parse(req.Query["totalUnits"]);
//        var batchSize = int.Parse(req.Query["batchSize"]);
//        var maxTripLength = int.Parse(req.Query["maxTripLength"]);
//        var minTripLength = int.Parse(req.Query["minTripLength"]);

//        log.LogInformation($"Starting simulation with {totalUnits} units");
//        var r = new Random();

//        int iCurrent = 0;
//        for (int iTotal = 0; iTotal < totalUnits; iTotal += batchSize)
//        {

//            for (int iBatch = 0; iBatch < batchSize; iBatch++)
//            {
//                var entityId = new EntityId(nameof(UnitEntity), iCurrent.ToString());
//                int tripLength = r.Next(minTripLength, maxTripLength);
//                await client.SignalEntityAsync(entityId, "Start", tripLength);
//                iCurrent++;
//            }

//            Thread.Sleep(2000);
//            log.LogInformation($"current total {iTotal}");
//        }

//        log.LogInformation($"all trips started");
//        return new OkObjectResult("V0.12 :)");
//    }
//}
