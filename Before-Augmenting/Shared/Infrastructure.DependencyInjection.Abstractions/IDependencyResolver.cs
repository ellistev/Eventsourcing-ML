namespace Infrastructure.DependencyInjection.Abstractions
{
    public interface IDependencyResolver
    {
        T Resolve<T>();
    }
}
