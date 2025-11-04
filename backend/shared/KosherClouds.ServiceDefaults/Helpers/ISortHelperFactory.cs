namespace KosherClouds.ServiceDefaults.Helpers
{
    public interface ISortHelperFactory
    {
        ISortHelper<T> Create<T>();
    }
}
