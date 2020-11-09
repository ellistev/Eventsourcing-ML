using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Generic;
using Inventory.Api.Queries;
using Inventory.Domain.Models.Commands;
using Inventory.Domain.ReadModels.Rooms;
using CQRS.Essentials.Abstractions.CQRS;

namespace Inventory.Api.Functions
{
    public class InventoryFunctions
    {
        private readonly IBus _bus;

        public InventoryFunctions(IBus bus)
        {
            _bus = bus;
        }

        [FunctionName("AddRoomFunction")]
        public async Task<IActionResult> AddRoom([HttpTrigger(AuthorizationLevel.Function, "post", Route = "inventory/room/add")] HttpRequest req, CancellationToken cancellationToken, ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var addRoomCommand = JsonConvert.DeserializeObject<AddRoom>(requestBody);  //you may want to use a mapper from dto to command instead (backwards compatibility)
                var events = await _bus.Send(addRoomCommand, cancellationToken);
                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, ex.Message, new Dictionary<string, object> { ["Request"] = req.Body });
                return new BadRequestObjectResult("Error occured on InventoryFunctions.AddRoomFunction.");
            }
        }

        [FunctionName("GetRoomByIdFunction")]
        public async Task<IActionResult> GetRoomById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "inventory/room/{hotelId}/{roomId}")] HttpRequest req, Guid hotelId, Guid roomId, CancellationToken cancellationToken, ILogger log)
        {
            try
            {
                var query = new FindRoomQuery { HotelId = hotelId, RoomId = roomId };
                var result = await _bus.Send<FindRoomQuery, RoomsReadModel>(query, cancellationToken);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, ex.Message, new Dictionary<string, object> { ["Request"] = req.Query.ToString() });
                return new BadRequestObjectResult("Error occured on InventoryFunctions.GetRoomById.");
            }
        }
    }
}
