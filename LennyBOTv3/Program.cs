using System.Globalization;
using LennyBOTv3.Services;
using LennyBOTv3.Settings;

namespace LennyBOTv3
{
    public static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            #region Culture

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            #endregion Culture

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
                // configuration
                var configurationRoot = context.Configuration;
                services.AddOptions<DiscordSettings>().Bind(configurationRoot.GetSection(DiscordSettings.SectionKey)).ValidateDataAnnotations();
                services.AddOptions<ApiSettings>().Bind(configurationRoot.GetSection(ApiSettings.SectionKey)).ValidateDataAnnotations();
                ApiSettings apiSettings = new();
                configurationRoot.GetSection(ApiSettings.SectionKey).Bind(apiSettings);

                // services
                services.AddHostedService<Bot>();
                services.AddHostedService<DatabaseService>();
                services.AddSingleton<Random>();
                services.AddSingleton<SearchService>();
                services.AddSingleton<RssService>();
                services.AddSingleton(new Google.Apis.YouTube.v3.YouTubeService(new() { ApiKey = apiSettings.YouTubeApiKey, ApplicationName = "LennyBOT" }));
                services.AddSingleton(new OMDbApiNet.AsyncOmdbClient(apiSettings.OmdbApiKey, true));
                FixerSharp.Fixer.SetApiKey(apiSettings.FixerSharpApiKey);
            });
    }
}
