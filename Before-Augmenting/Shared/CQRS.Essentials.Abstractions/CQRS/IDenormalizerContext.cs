using Infrastructure.Storage.Abstractions;

namespace CQRS.Essentials.Abstractions.CQRS
{
    public interface IDenormalizerContext<TModel>
    {
        IRepository<TModel> Repository { get; }
        IRepository<TLookup> Lookup<TLookup>();
    }
}