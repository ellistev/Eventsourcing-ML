using CQRS.Essentials.Abstractions.CQRS;
using Reservations.Domain.Models.Events;
using System.Threading;
using System.Threading.Tasks;
using Reservations.Domain.ReadModels.RoomViews;

namespace Reservations.Domain.ReadModels.Room
{
    public class RoomViewsDenormalizer
    {
        public RoomViewsDenormalizer(IBuilder builder)
        {
            builder.RegisterDenormalizer(new DenormalizerDesc(typeof(RoomViewsReadModel)));
            builder.RegisterEventHandler<RoomViewsReadModel, RoomViewed>(OnRoomViewed);
            builder.RegisterEventHandler<RoomViewsReadModel, ReservationMade>(OnReservationMade);
        }

        private async Task OnRoomViewed(IDenormalizerContext<RoomViewsReadModel> ctx, RoomViewed @event)
        {

            
            var view = new RoomViewsReadModel
            {
                HotelId = @event.HotelId,
                ViewId = @event.ViewId,
                UserId = @event.UserId,
                RoomType = @event.RoomType,
                ViewDateTime = @event.TimeStamp
            };
            await ctx.Repository.Save(view, new CancellationToken());        
        }
    
        private async Task OnReservationMade(IDenormalizerContext<RoomViewsReadModel> ctx, ReservationMade @event)
        {
            var view = await ctx.Repository.GetByIds(@event.HotelId, @event.ViewId, CancellationToken.None);

            view.HotelId = @event.HotelId;
            view.ViewId = @event.ViewId;
            view.ReserveDateTime = @event.Timestamp;
            
            await ctx.Repository.Save(view, new CancellationToken());        

        }
    }
}