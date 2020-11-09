using CQRS.Essentials.Abstractions.CQRS;
using Reservations.Domain.Models.Events;
using System.Threading;
using System.Threading.Tasks;
using Reservations.Domain.ReadModels.LocationWeather;

namespace Reservations.Domain.ReadModels.LocationWeather
{
    public class LocationWeatherDenormalizer
    {
        public LocationWeatherDenormalizer(IBuilder builder)
        {
            builder.RegisterDenormalizer(new DenormalizerDesc(typeof(LocationWeatherReadModel)));
            builder.RegisterEventHandler<LocationWeatherReadModel, ReservationMade>(OnReservationMade);
        }

        private async Task OnReservationMade(IDenormalizerContext<LocationWeatherReadModel> ctx, ReservationMade @event)
        {

        }
    }
}