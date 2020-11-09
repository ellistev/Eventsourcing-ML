using CQRS.Essentials.Abstractions.CQRS;
using Reservations.Domain.Models.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Reservations.Domain.ReadModels.Room
{
    public class RoomsDenormalizer
    {
        public RoomsDenormalizer(IBuilder builder)
        {
            builder.RegisterDenormalizer(new DenormalizerDesc(typeof(RoomsReadModel)));
            builder.RegisterEventHandler<RoomsReadModel, ReservationMade>(OnReservationMade);
        }

        private async Task OnReservationMade(IDenormalizerContext<RoomsReadModel> ctx, ReservationMade @event)
        {

        }
    }
}