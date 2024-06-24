using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.Implementation.Events;
using TicketApp.Common.DTOs;
using TicketApp.Common.Extensions;
using TicketApp.Entities;
using TicketApp.Entities.Enums;

namespace TicketApp.BusinessLogic.Implementation.Locations
{
    public class LocationService : BaseService
    {
        private readonly CreateLocationValidator CreateLocationValidator;

        public LocationService(ServiceDependencies serviceDependencies) : base(serviceDependencies)
        {
            CreateLocationValidator = new CreateLocationValidator();
        }

        public Guid CreateLocation(CreateLocationModel model)
        {
            CreateLocationValidator.Validate(model).ThenThrow(model);

            var newLocation = Mapper.Map<CreateLocationModel, Location>(model);

            UnitOfWork.Locations.Insert(newLocation);

            UnitOfWork.SaveChanges();

            return newLocation.Id;
        }

        public async Task<(decimal, decimal)> GetLocationCoordinates(Guid id)
        {
            var locationCoordinates = await UnitOfWork.Locations
                .Get()
                .Where(l => l.Id == id)
                .Select(l => new { l.Latitude, l.Longitude})
                .SingleOrDefaultAsync();
            return (locationCoordinates.Latitude, locationCoordinates.Longitude);
        }

        public async Task<List<LocationMapModel>> GetLocationsForMap(string mapType, string swLatString, string swLngString, string neLatString, string neLngString, 
            string eventType, string startDate) 
        {
            var parseResult = Boolean.TryParse(mapType, out var mapTypeToBool);
            bool parseDateResult;

            decimal swLat = decimal.Parse(swLatString);
            decimal swLng = decimal.Parse(swLngString);
            decimal neLat = decimal.Parse(neLatString);
            decimal neLng = decimal.Parse(neLngString);

            eventType = char.ToLower(eventType[0]) + eventType.Substring(1);

            var eventTypeResult = Int32.TryParse(eventType, out var eventTypeToInt); 

            DateTime startDateToDate = DateTime.Now;
            if (startDate != "all")
                parseDateResult = DateTime.TryParse(startDate, out startDateToDate);


            if (parseResult == false)
                return null;
            var locations = await UnitOfWork.Locations
                .Get()
                .Include(l => l.Events)
                    .ThenInclude(e => e.EventNavigation)
                        .ThenInclude(e => e.EventType)
                .Where(e => e.Events.Any(ev => ev.EventNavigation.EventType.IsNightEventType == mapTypeToBool)
                    && (eventType != "all" ? e.Events.Any(ev => ev.EventNavigation.EventType.Id == eventTypeToInt) : true)
                    && (e.Events.Any(ev => ev.StartDate >= startDateToDate))
                    && (e.Events.Any(ev => ev.Artists.All(a => a.Status == (int)StatusCode.Approved) || ev.Artists.Count() == 0))
                    )
                .Where(l => (l.Latitude >= swLat && l.Latitude <= neLat && l.Longitude >= swLng && l.Longitude <= neLng) && l.Status == (int)StatusCode.Approved)
                .Include(l => l.LocationType)
                .Include(l => l.Events)
                    .ThenInclude(e => e.Picture)
                .ToListAsync();

            var locationMapModels = new List<LocationMapModel>();

            foreach(var loc in locations)
            {
                loc.Events = loc.Events
                    .Where(ev => ev.EventNavigation.EventType.IsNightEventType == mapTypeToBool 
                        && (eventType != "all" ?  ev.EventNavigation.EventType.Id == eventTypeToInt : true)
                        && (ev.StartDate >= startDateToDate)
                        && (ev.Artists.All(a => a.Status == (int)StatusCode.Approved) || ev.Artists.Count() == 0))
                    .OrderBy(ev => ev.StartDate)
                    .ToList();
                locationMapModels.Add(Mapper.Map<LocationMapModel>(loc));
            }
            return locationMapModels;
        }
    }
}
