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
            builder.RegisterDenormalizer(new DenormalizerDesc(typeof(RoomTemperatureViewsReadModel), new [] {typeof(LocationWeather.LocationWeatherReadModel), typeof(Hotel.HotelsReadModel), typeof(User.UsersReadModel)}));
            builder.RegisterEventHandler<RoomTemperatureViewsReadModel, RoomViewed>(OnRoomViewed);
            builder.RegisterEventHandler<RoomTemperatureViewsReadModel, ReservationMade>(OnReservationMade);
        }

        private async Task OnRoomViewed(IDenormalizerContext<RoomTemperatureViewsReadModel> ctx, RoomViewed @event)
        {
            var hotel = await ctx.Lookup<Hotel.HotelsReadModel>().GetByIds(@event.HotelId, @event.HotelId, CancellationToken.None);
            var weatherForHotel = await ctx.Lookup<LocationWeather.LocationWeatherReadModel>().GetByKeys(hotel.Location, @event.TimeStamp.Date.ToString("yyyy-MM-dd"), CancellationToken.None);
            
            var user = await ctx.Lookup<User.UsersReadModel>().GetByIds(@event.UserId, @event.UserId, CancellationToken.None);
            var weatherForUser = await ctx.Lookup<LocationWeather.LocationWeatherReadModel>().GetByKeys(user.Location, @event.TimeStamp.Date.ToString("yyyy-MM-dd"), CancellationToken.None);

            var reservation = new RoomTemperatureViewsReadModel
            {
                HotelId = @event.HotelId,
                ViewId = @event.ViewId,
                ReservationDateTime = null,
                ReservationId = @event.ReservationId,
                ViewDateTime = @event.TimeStamp,
                HotelLocationTemp = weatherForHotel.Temperature,
                UserLocationTemp = weatherForUser.Temperature,
                RoomType = @event.RoomType
            };
            await ctx.Repository.Save(reservation, new CancellationToken());

        }
        
        private async Task OnReservationMade(IDenormalizerContext<RoomTemperatureViewsReadModel> ctx, ReservationMade @event)
        {
            var view = await ctx.Repository.GetByIds(@event.HotelId, @event.ViewId, CancellationToken.None);

            view.ReservationDateTime = @event.Timestamp;
            view.MillisecondsToReservation = (@event.Timestamp - view.ViewDateTime).TotalMilliseconds;
            
            await ctx.Repository.Save(view, new CancellationToken());

        }
    }
}