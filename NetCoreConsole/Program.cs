using System.Threading.Tasks;
using AppInsightsLogging;
using Azure.Storage.Blobs;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetCoreConsole
{
	internal class Program
	{
		private static async Task Main(string[] args)
		{
			IHostBuilder defaultBuilder = Host.CreateDefaultBuilder(args);
			defaultBuilder.ConfigureServices((context, services) =>
			{
				IConfiguration configuration = context.Configuration;

				services.Configure<ApplicationInsightsSettings>(options =>
					configuration.GetSection("ApplicationInsights").Bind(options));
			});

			await defaultBuilder
				.ConfigureLogging(builder =>
				{
					ServiceProvider provider = builder.Services.BuildServiceProvider();
					IOptions<ApplicationInsightsSettings> appInsightsSettings = provider.GetRequiredService<IOptions<ApplicationInsightsSettings>>();
					builder.AddApplicationInsights(appInsightsSettings.Value.InstrumentationKey);
				})
				.ConfigureServices((context, services) =>
				{
					IConfiguration configuration = context.Configuration;
					services.Configure<StorageSettings>(options => configuration.GetSection("StorageSettings").Bind(options));


					services.AddSingleton(provider =>
					{
						IOptions<StorageSettings> storageSettings = provider.GetRequiredService<IOptions<StorageSettings>>();
						return new BlobServiceClient(storageSettings.Value.Connection);
					});

					services.AddHostedService<MyService>();

					////////https://docs.microsoft.com/en-us/azure/azure-monitor/app/worker-service
					////ServiceProvider provider = services.BuildServiceProvider();

					////IOptions<ApplicationInsightsSettings> appInsightsSettings = provider.GetRequiredService<IOptions<ApplicationInsightsSettings>>();

					////services.AddApplicationInsightsTelemetryWorkerService(appInsightsSettings.Value.InstrumentationKey);
				})
				.RunConsoleAsync();
		}
	}
}
