using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NetFrameworkConsole
{
	internal class MyService
	{
		private readonly ILogger<MyService> _logger;

		public MyService(
			ILogger<MyService> logger
			)
		{
			_logger = logger;
		}

		public void Start()
		{
			DateTime timeStamp = DateTime.Now;
			using (_logger.BeginScope($"Starting application {timeStamp}..."))
			{
				try
				{
					_logger.LogInformation($"Hello from .NET Framework! {timeStamp}");

					// Simulate real work is being done
					Thread.Sleep(1000);

					_logger.LogInformation($"Job done!!! {timeStamp}");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Unhandled exception!");
				}
			}
		}
	}
}
