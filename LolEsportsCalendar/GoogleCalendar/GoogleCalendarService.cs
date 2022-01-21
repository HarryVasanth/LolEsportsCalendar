﻿using Google.Apis.Calendar.v3.Data;
using GoogleCalendarApiClient.Services;
using LolEsportsApiClient.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace LolEsportsCalendar.GoogleCalendar
{
	public class GoogleCalendarService
	{
		private readonly Dictionary<string, string> calendarLookup = new Dictionary<string, string>();

		private CalendarsService _calendarsService;
		private CalendarListService _calendarListService;
		private ILogger<GoogleCalendarService> _logger;

		public GoogleCalendarService(CalendarsService calendarsService, CalendarListService calendarListService, ILogger<GoogleCalendarService> logger)
		{
			_logger = logger;
			_calendarsService = calendarsService;
			_calendarListService = calendarListService;
			_eventsService = eventsService;

			BuildCalendarLookup();
		}

		public void BuildCalendarLookup()
		{
			// View
			CalendarList calendarList = _calendarListService.List();

			// Add calendars to lookup
			foreach (var c in calendarList.Items)
			{
				// calendarLookup.Add(c.Id, c.Summary);
				calendarLookup.Add(c.Summary, c.Id);
			}
		}

		public string FindCalendarId(string leagueName)
		{
			return calendarLookup[leagueName];
		}

		public bool CalendarExists(string key)
		{
			return calendarLookup.ContainsKey(key);
		}

		public Calendar InsertLeagueAsCalendar(League league)
		{
			Calendar newCalendar = null;
			Calendar calendar = new Calendar
			{
				Summary = league.Name,
				Description = league.Name + " / " + league.Region,
				// ETag = "Test",
				// Kind = "Test",
				// Location = "Test",
				// TimeZone = "Europe/Amsterdam"
			};

			try
			{
				newCalendar = _calendarsService.Insert(calendar);
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Error while creating calendar", calendar.Summary);
			}

			if (newCalendar != null)
			{
				_logger.LogInformation("Created calendar {0}", calendar.Summary);
				calendarLookup.Add(calendar.Summary, calendar.Id);
			}

			return newCalendar;
		}

		public Event ConvertEsportEventToGoogleEvent(EsportEvent esportEvent)
		{
			// Convert EsportEvent to Event
			Event @event = new Event()
			{
				Id = esportEvent.Match.Id,
				// ICalUID = esportEvent.Match.Id,
				Start = new EventDateTime { DateTime = esportEvent.StartTime.UtcDateTime },
				End = new EventDateTime { DateTime = esportEvent.StartTime.UtcDateTime },
				Summary = esportEvent.Match.Teams[0].Code + " vs " + esportEvent.Match.Teams[1].Code
			};

			return @event;
		}
	}
}
