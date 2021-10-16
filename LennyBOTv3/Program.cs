using LennyBOTv3.Settings;

namespace LennyBOTv3
{
    internal static class Program
    {
        static async Task<int> Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            var logger = host.Services.GetRequiredService<ILogger<IHost>>();

            try
            {
                await host.RunAsync();
            }
            catch (Exception e)
            {
                logger.LogCritical("Unhandled exception:\n{Exception}", e);
                return -1;
            }

            return 0;
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .ConfigureServices((context, services) =>
            {
                // services
                services.AddHostedService<Bot>();

                // configuration
                var configurationRoot = context.Configuration;
                services.AddOptions<DiscordSettings>().Bind(configurationRoot.GetSection(DiscordSettings.SectionKey)).ValidateDataAnnotations();
                //services.AddOptions<OutputOptions>().Bind(configurationRoot.GetSection(OutputOptions.SectionKey)).ValidateDataAnnotations();
            });
    }
}
