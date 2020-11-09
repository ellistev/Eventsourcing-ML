using CQRS.Essentials.Abstractions.CQRS;
using Inventory.Domain.Models.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Domain.ReadModels.Rooms
{
    public class RoomsDenormalizer
    {
        public RoomsDenormalizer(IBuilder builder)
        {
            builder.RegisterDenormalizer(new DenormalizerDesc(typeof(RoomsReadModel)));
            builder.RegisterEventHandler<RoomsReadModel, RoomAdded>(OnRoomAdded);
        }

        private async Task OnRoomAdded(IDenormalizerContext<RoomsReadModel> ctx, RoomAdded @event)
        {
            var room = new RoomsReadModel
            {
                Id = @event.Id,
                HotelId = @event.HotelId,
                RoomType = @event.RoomType
            };
            await ctx.Repository.Save(room, new CancellationToken());
        }
    }
}