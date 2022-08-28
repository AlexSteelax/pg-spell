using Serilog;
using Steelax.PgSpell.Helpers;
using Steelax.PgSpell.Services;
using Steelax.PgSpell.Settings;

namespace Steelax.PgSpell
{
    public class Program
    {
        public static Task Main(string[] args)
            => CreateHostBuilder(args).Build().StartAsync();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((hostContext, logger) =>
                {
                    logger.WriteTo.Console().MinimumLevel.Information();
                })
                .ConfigureDefaults(args)
                .UseConsoleLifetime(config => config.SuppressStatusMessages = true)
                .ConfigureServices((hostContext, services) =>
                {
                    services.ConfigureSettings<SqlGenerateSettings>(hostContext.Configuration);
                    services.AddHostedService<MainHostedService>();
                });
    }
}