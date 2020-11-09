using CQRS.Essentials.Abstractions.CQRS;
using Reservations.Domain.Models.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Reservations.Domain.ReadModels.Hotel
{
    public class HotelsDenormalizer
    {
        public HotelsDenormalizer(IBuilder builder)
        {
            builder.RegisterDenormalizer(new DenormalizerDesc(typeof(HotelsReadModel)));
            builder.RegisterEventHandler<HotelsReadModel, ReservationMade>(OnReservationMade);
        }

        private async Task OnReservationMade(IDenormalizerContext<HotelsReadModel> ctx, ReservationMade @event)
        {

        }
    }
}