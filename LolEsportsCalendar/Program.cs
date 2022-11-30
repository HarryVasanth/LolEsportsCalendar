﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LolEsportsCalendar
{
	class Program
	{
		static async Task Main(string[] args)
		{
			// Get app configuration
			ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
			var configuration = configurationBuilder.AddJsonFile("appsettings.json").Build();

			// Collect app services
			var serviceCollection = new ServiceCollection();

			// Register console app
			serviceCollection.AddSingleton<ConsoleApp>();

			// Register logging
			serviceCollection.AddLogging(config => {
				config.AddConsole().AddConfiguration(configuration.GetSection("Logging"));
			});

			// Configure servicesd
			ConfigureServices(serviceCollection, configuration);

			// Run
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var consoleApp = serviceProvider.GetRequiredService<ConsoleApp>();
            await consoleApp.RunAsync();
		}

		public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		{
			services.AddSingleton<IConfiguration>(_ => configuration);

			// Google Calendar API
			services.AddGoogleCalendarService();

			// LolEsports API
			services.AddLeagueEsportService(configuration.GetSection("LolEsports"));
		}
	}
}
