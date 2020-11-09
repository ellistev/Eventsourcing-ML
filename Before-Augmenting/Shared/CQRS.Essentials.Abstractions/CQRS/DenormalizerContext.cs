using Infrastructure.Storage.Abstractions;
using System.Collections.Generic;

namespace CQRS.Essentials.Abstractions.CQRS
{
    public class DenormalizerContext<TModel> : IDenormalizerContext<TModel>
    {
        private readonly IDictionary<string, object> _lookups;

        public DenormalizerContext(IRepository<TModel> repository, IDictionary<string, object> lookups)
        {
            Repository = repository;
            _lookups = lookups;
        }
        
        public IRepository<TModel> Repository { get; }
        public IRepository<TLookup> Lookup<TLookup>()
        {
            return (IRepository<TLookup>) _lookups[typeof(TLookup).FullName];
        }
    }
}