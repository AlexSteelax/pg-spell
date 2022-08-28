namespace Steelax.PgSpell.Helpers
{
    public static class ServiceConfigure
    {
        public static void ConfigureSettings<TOption>(this IServiceCollection services, IConfiguration config) where TOption : class
        {
            services.Configure<TOption>(config.GetSection(typeof(TOption).Name));
        }
    }
}
