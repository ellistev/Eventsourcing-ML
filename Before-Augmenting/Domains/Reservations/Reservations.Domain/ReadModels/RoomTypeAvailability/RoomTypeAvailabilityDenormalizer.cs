using CQRS.Essentials.Abstractions.CQRS;
using Reservations.Domain.Models.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Reservations.Domain.ReadModels.Reservation
{
    public class RoomTypeAvailabilityDenormalizer
    {
        public RoomTypeAvailabilityDenormalizer(IBuilder builder)
        {
            builder.RegisterDenormalizer(new DenormalizerDesc(typeof(RoomTypeAvailabilityReadModel)));
            builder.RegisterEventHandler<RoomTypeAvailabilityReadModel, RoomTypeAvailabilityIncreased>(OnRoomTypeAvailabilityIncreased);
            builder.RegisterEventHandler<RoomTypeAvailabilityReadModel, RoomTypeAvailabilityDecreased>(OnRoomTypeAvailabilityDecreased);
        }

        private async Task OnRoomTypeAvailabilityIncreased(IDenormalizerContext<RoomTypeAvailabilityReadModel> ctx, RoomTypeAvailabilityIncreased @event)
        {
            var roomTypeAvailability = await ctx.Repository.GetByKeys(@event.Id.ToString(), @event.RoomType, new CancellationToken());
            if(roomTypeAvailability == null)
            {
                roomTypeAvailability = new RoomTypeAvailabilityReadModel { HotelId = @event.Id, RoomType = @event.RoomType, Amount = @event.Amount };
            }
            else
            {
                roomTypeAvailability.Amount += @event.Amount;
            }
            await ctx.Repository.Save(roomTypeAvailability, new CancellationToken());
        }

        private async Task OnRoomTypeAvailabilityDecreased(IDenormalizerContext<RoomTypeAvailabilityReadModel> ctx, RoomTypeAvailabilityDecreased @event)
        {
            var roomTypeAvailability = await ctx.Repository.GetByKeys(@event.Id.ToString(), @event.RoomType, new CancellationToken());
            if (roomTypeAvailability == null)
            {
                roomTypeAvailability = new RoomTypeAvailabilityReadModel { HotelId = @event.Id, RoomType = @event.RoomType, Amount = @event.Amount };
            }
            else
            {
                roomTypeAvailability.Amount -= @event.Amount;
            }
            await ctx.Repository.Save(roomTypeAvailability, new CancellationToken());
        }
    }
}