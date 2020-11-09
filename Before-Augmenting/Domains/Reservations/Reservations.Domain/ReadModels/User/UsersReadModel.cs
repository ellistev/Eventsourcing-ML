using Infrastructure.Storage.Abstractions.CustomAttributes;
using System;

namespace Reservations.Domain.ReadModels.User
{
    public class UsersReadModel
    {
        [PartitionKey]
        public Guid UserId { get; set; }
        [RowKey]
        public Guid Id { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
    }
}