using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System;
using HotelManagement.Processor.Handlers;
using CQRS.Essentials.Abstractions.CQRS;

namespace HotelManagement.Processor
{
    public class ProcessorFunctions
    {
        private readonly IBus _bus;
        public ProcessorFunctions(IBus bus)
        {
            _bus = bus;
        }

        [FunctionName("OnInventoryEventsProcessor")]
        public async Task OnInventoryEventsProcessor([ServiceBusTrigger("%INVENTORY_EVENTS_TOPIC_NAME%", "%INVENTORY_RESERVATIONS_SUBSCRIPTION_NAME%", Connection = "HOTEL_MANAGEMENT_SERVICEBUS")]Message message,
        CancellationToken cancellationToken, ILogger log)
        {
            try
            {
                var serviceBusMessageHandler = new BusMessageHandler(_bus);
                await serviceBusMessageHandler.Handle(message, cancellationToken);
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, ex.Message);
            }
        }
    }
}
