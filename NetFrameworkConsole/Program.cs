using System.Threading;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetFrameworkConsole
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (ITelemetryChannel channel = new ServerTelemetryChannel())
			{
				try
				{
					IConfiguration configuration = new ConfigurationBuilder()
						.AddJsonFile($"appsettings.json", true)
						.AddEnvironmentVariables()
						.Build();

					IServiceCollection services = new ServiceCollection();

					string appInsightsInstrumentationKey =
						configuration.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");
					if (!string.IsNullOrEmpty(appInsightsInstrumentationKey))
					{
						services.AddSingleton(serviceProvider =>
							new ApplicationInsightsSettings { InstrumentationKey = appInsightsInstrumentationKey });
					}
					else
					{
						services.Configure<ApplicationInsightsSettings>(options =>
							configuration.GetSection("ApplicationInsights").Bind(options));
					}

					ServiceProvider provider = services.BuildServiceProvider();
					IOptions<ApplicationInsightsSettings> appInsightsSettings = provider.GetRequiredService<IOptions<ApplicationInsightsSettings>>();

					services.Configure<TelemetryConfiguration>(config =>
					{
						config.InstrumentationKey = appInsightsSettings.Value.InstrumentationKey;
						config.TelemetryChannel = channel;
					});

					services.AddLogging(builder =>
					{
						builder.AddApplicationInsights(appInsightsSettings.Value.InstrumentationKey);
						builder.AddConsole();
						builder.AddEventSourceLogger();
					});

					services.AddTransient<MyService>();

					provider = services.BuildServiceProvider();
					MyService myService = provider.GetRequiredService<MyService>();
					myService.Start();
				}
				finally
				{
					channel.Flush();

					//Wait for it to flush
					Thread.Sleep(1000);
				}
			}
		}
	}
}
