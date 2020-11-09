using System;
using System.Collections.Generic;

namespace CQRS.Essentials.Abstractions.CQRS
{
    public class DenormalizerDesc
    {
        public DenormalizerDesc(Type model, params Type[] lookups)
        {
            Model = model;
            Lookups = lookups;
        }

        public Type Model { get; }
        public IEnumerable<Type> Lookups { get; }
    }
}