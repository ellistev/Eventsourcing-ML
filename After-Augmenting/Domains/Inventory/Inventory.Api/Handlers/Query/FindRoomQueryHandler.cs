using System.Threading;
using System.Threading.Tasks;
using CQRS.Essentials.Abstractions.CQRS;
using Inventory.Api.Queries;
using Inventory.Domain.ReadModels.Rooms;
using Infrastructure.Storage.Abstractions;

namespace Inventory.Api.Handlers.Query
{
    public class FindRoomQueryHandler : IQueryHandler<FindRoomQuery, RoomsReadModel>
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public FindRoomQueryHandler(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public async Task<RoomsReadModel> Handle(FindRoomQuery query, CancellationToken cancellationToken)
        {
            var repo = _repositoryFactory.Create<RoomsReadModel>();
            return await repo.GetByIds(query.HotelId, query.RoomId, cancellationToken);
        }
    }
}
