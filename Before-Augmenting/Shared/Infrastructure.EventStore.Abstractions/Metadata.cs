using System.Collections.Generic;

namespace Infrastructure.EventStore.Abstractions
{
    public class Metadata : IMetadataProvider
    {
        private readonly IDictionary<string, object> _properties;

        public IDictionary<string, object> Value => new Dictionary<string, object>(_properties);

        public Metadata()
        {
            _properties = new Dictionary<string, object>();
        }

        public Metadata(IDictionary<string, object> metadata)
        {
            _properties = new Dictionary<string, object>(metadata);
        }

        public void Add(string key, object value)
        {
            _properties.Add(key, value);
        }

        public bool TryGet<T>(string name, out T value)
        {
            value = default(T);
            if (!_properties.TryGetValue(name, out object rawValue)) return false;
            value = (T)rawValue;
            return true;
        }
        
        public static implicit operator Metadata(Dictionary<string, object> source)
        {
            return new Metadata(source);
        }
    }
}