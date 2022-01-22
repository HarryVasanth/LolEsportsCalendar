﻿using Google.Apis.Calendar.v3.Data;
using GoogleCalendarApiClient.Services;
using LolEsportsApiClient;
using LolEsportsApiClient.Models;
using LolEsportsCalendar.GoogleCalendar;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LolEsportsCalendar.LolEsports
{
	public class LolEsportsService
	{
		private readonly LolEsportsClient _lolEsportsClient;
		private readonly LolEsportsOptions _options;
		private readonly GoogleCalendarService _googleCalendarService;
		private readonly CalendarsService _calendarsService;
		private readonly EventsService _eventsService;
		private readonly ILogger<LolEsportsService> _logger;

		public LolEsportsService(
			GoogleCalendarService googleCalendarService,
			LolEsportsClient lolEsportsClient,
			EventsService eventsService,
			CalendarsService calendarsService,
			ILogger<LolEsportsService> logger,
			IOptions<LolEsportsOptions> options
		)
		{
			_googleCalendarService = googleCalendarService;
			_calendarsService = calendarsService;
			_eventsService = eventsService;
			_lolEsportsClient = lolEsportsClient;
			_logger = logger;
			_options = options.Value;
		}

		public async Task ImportEvents()
		{
			string[] leagueNames = _options.Leagues;

			if (leagueNames != null) 
			{
				foreach(var leagueName in leagueNames)
				{
					Calendar calendar = FindOrCreateCalendarByLeagueName(leagueName);
					League league = _lolEsportsClient.GetLeagueByName(leagueName);

					// Import events for calendar
					await ImportEventsForLeagueAsync(league.Id, calendar.Id);
				}
			} else {
				await ImportEventsForAllCalendarsAsync();
			}
		}

		public async Task ImportEventsForAllCalendarsAsync()
		{
			try
			{
				List<League> leagues = await _lolEsportsClient.GetLeaguesAsync();

				if (leagues == null)
				{
					throw new NullReferenceException();
				}

				foreach (League league in leagues)
				{
					Calendar calendar = FindOrCreateCalendarByLeagueName(league.Name);

					// Import events for calendar
					await ImportEventsForLeagueAsync(league.Id, calendar.Id);
				}
			}
			catch (Exception exception)
			{
				_logger.LogError("Error while importing events for all calendars", exception);
			}
		}

		public async Task ImportEventsForLeagueAsync(string leagueId, string calendarId)
		{
			try
			{
				// Get scheduled events of league
				List<EsportEvent> esportEvents = await _lolEsportsClient.GetScheduleByLeagueAsync(leagueId);

				foreach (EsportEvent esportEvent in esportEvents)
				{
					// Convert LeagueEvent to GoogleEvent
					Event googleEvent = _googleCalendarService.ConvertEsportEventToGoogleEvent(esportEvent);

					// Insert or Update GoogleEvent
					_eventsService.InsertOrUpdate(googleEvent, calendarId, googleEvent.Id);
				}
			}
			catch (Exception exception)
			{
				_logger.LogError("Error while importing events for leauge {0}", exception, leagueId);
			}

			_logger.LogInformation("Events imported for league {0}", leagueId);
		}

		public Calendar FindOrCreateCalendarByLeagueName(string leagueName)
		{
			// Find calendar
			string calendarId = _googleCalendarService.FindCalendarId(leagueName);
			Calendar existingCalendar = _calendarsService.Get(calendarId);

			if (existingCalendar == null)
			{
				League league = _lolEsportsClient.GetLeagueByName(leagueName);
				Calendar newCalendar = ConvertLeagueToCalendar(league);

				return _calendarsService.Insert(newCalendar);
			}

			return existingCalendar;
		}

		public Calendar ConvertLeagueToCalendar(League league)
		{
			Calendar calendar = new Calendar
			{
				Summary = league.Name,
				Description = league.Name + " / " + league.Region,
				// ETag = "Test",
				// Kind = "Test",
				// Location = "Test",
				// TimeZone = "Europe/Amsterdam"
			};

			return calendar;
		}
	}
}
