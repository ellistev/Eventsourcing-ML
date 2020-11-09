using CQRS.Essentials.Abstractions.CQRS;
using Reservations.Domain.Models.Events;
using System.Threading;
using System.Threading.Tasks;
using Reservations.Domain.ReadModels.Room;

namespace Reservations.Domain.ReadModels.RoomTemperatureViews
{
    public class RoomTemperatureViewsDenormalizer
    {
        public RoomTemperatureViewsDenormalizer(IBuilder builder)
        {
            builder.RegisterDenormalizer(new DenormalizerDesc(typeof(RoomTemperatureViewsReadModel)));
            builder.RegisterEventHandler<RoomTemperatureViewsReadModel, ReservationMade>(OnReservationMade);
        }

        private async Task OnReservationMade(IDenormalizerContext<RoomTemperatureViewsReadModel> ctx, ReservationMade @event)
        {

        }
    }
}