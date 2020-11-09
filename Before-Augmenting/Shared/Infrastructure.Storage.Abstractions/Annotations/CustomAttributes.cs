using System;

namespace Infrastructure.Storage.Abstractions.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PartitionKeyAttribute : Attribute{}
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RowKeyAttribute : Attribute { }
}
