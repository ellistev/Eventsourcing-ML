using System.Threading;
using System.Threading.Tasks;

namespace CQRS.Essentials.Abstractions.CQRS
{
    public interface ICommandHandler<in T> where T : ICommand
    {
        Task<object[]> Handle(T command, CancellationToken cancellationToken);
    }
}
