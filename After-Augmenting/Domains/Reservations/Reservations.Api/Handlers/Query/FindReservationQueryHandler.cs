using System.Threading;
using System.Threading.Tasks;
using CQRS.Essentials.Abstractions.CQRS;
using Infrastructure.Storage.Abstractions;
using Reservations.Api.Queries;
using Reservations.Domain.ReadModels.Reservation;

namespace Reservations.Api.Handlers.Query
{
    public class FindReservationQueryHandler : IQueryHandler<FindReservationQuery, ReservationsReadModel>
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public FindReservationQueryHandler(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public async Task<ReservationsReadModel> Handle(FindReservationQuery query, CancellationToken cancellationToken)
        {
            var repo = _repositoryFactory.Create<ReservationsReadModel>();
            return await repo.GetByIds(query.HotelId, query.ReservationId, cancellationToken);
        }
    }
}
