using Infrastructure.EventStore.Abstractions;
using System.Threading.Tasks;

namespace Infrastructure.Publishers.Abstractions
{
    public interface IPublisher
    {
        Task Publish(IEventData eventData);
    }
}
