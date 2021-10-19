using System.Globalization;
using Google.Apis.YouTube.v3;
using LennyBOTv3.Services;
using LennyBOTv3.Settings;

namespace LennyBOTv3
{
    internal static class Program
    {
        static async Task<int> Main(string[] args)
        {
            #region Culture
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            #endregion
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
                services.AddSingleton<Random>();
                services.AddSingleton<SearchService>();
                services.AddSingleton(new YouTubeService(new() { ApiKey = apiSettings.YouTubeApiKey, ApplicationName = "LennyBOT" }));
                FixerSharp.Fixer.SetApiKey(apiSettings.FixerSharpApiKey);
            });
    }
}
