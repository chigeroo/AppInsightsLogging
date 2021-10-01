using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCoreConsole;

namespace AppInsightsLogging
{
	internal class MyService : IHostedService
	{
		private readonly ILogger<MyService> _logger;
		private readonly IHostApplicationLifetime _appLifetime;
		private readonly StorageSettings _storageSettings;
		private readonly BlobServiceClient _blobServiceClient;

		public MyService(
			ILogger<MyService> logger,
			IHostApplicationLifetime appLifetime,
			IOptions<StorageSettings> storageSettings,
			BlobServiceClient blobServiceClient)
		{
			_logger = logger;
			_appLifetime = appLifetime;
			_storageSettings = storageSettings.Value;
			_blobServiceClient = blobServiceClient;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_appLifetime.ApplicationStarted.Register(() =>
			{
				Task.Run(async () =>
				{
					DateTime timeStamp = DateTime.Now;
					using (_logger.BeginScope($"Starting application {timeStamp}..."))
					{
						try
						{
							_logger.LogInformation($"Hello from .NET Core! {timeStamp}");

							// Simulate real work is being done
							await Task.Delay(1000, cancellationToken);
							BlobContainerClient containerClient =
								_blobServiceClient.GetBlobContainerClient(_storageSettings.Container);
							await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

							_logger.LogInformation($"Job done!!! {timeStamp}");
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "Unhandled exception!");
						}
						finally
						{
							// Stop the application once the work is done
							_appLifetime.StopApplication();
						}
					}
				}, cancellationToken);
			});

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
