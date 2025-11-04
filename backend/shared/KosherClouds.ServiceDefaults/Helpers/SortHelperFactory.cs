using Microsoft.Extensions.DependencyInjection;

namespace KosherClouds.ServiceDefaults.Helpers
{
    public class SortHelperFactory : ISortHelperFactory
    {
        public ISortHelper<T> Create<T>()
        {
            return new SortHelper<T>();
        }
    }
}
