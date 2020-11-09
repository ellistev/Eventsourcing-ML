using CQRS.Essentials.Abstractions.CQRS;
using Reservations.Domain.Models.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Reservations.Domain.ReadModels.User
{
    public class UsersDenormalizer
    {
        public UsersDenormalizer(IBuilder builder)
        {
            builder.RegisterDenormalizer(new DenormalizerDesc(typeof(UsersReadModel)));
            builder.RegisterEventHandler<UsersReadModel, ReservationMade>(OnReservationMade);
        }

        private async Task OnReservationMade(IDenormalizerContext<UsersReadModel> ctx, ReservationMade @event)
        {

        }
    }
}