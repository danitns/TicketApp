using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI_API;
using OpenAI_API.Completions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.Implementation.Artists;
using TicketApp.BusinessLogic.Implementation.Events;
using TicketApp.BusinessLogic.Implementation.Events.Models;
using TicketApp.BusinessLogic.Implementation.Locations;
using TicketApp.BusinessLogic.Implementation.Tickets;
using TicketApp.Common.DTOs;
using TicketApp.Common.Exceptions;
using TicketApp.Common.Extensions;
using TicketApp.Entities;
using TicketApp.Entities.Enums;

namespace TicketApp.BusinessLogic.Implementation.Events
{
    public class EventService : BaseService
    {
        private readonly CreateEventValidator CreateEventValidator;

        private readonly EditEventValidator EditEventValidator;

        static private FilterEventModel FilterEventModel;

        private readonly IConfiguration Configuration;

        public EventService(ServiceDependencies serviceDependencies, IConfiguration configuration) : base(serviceDependencies)
        {
            CreateEventValidator = new CreateEventValidator();
            EditEventValidator = new EditEventValidator();
            Configuration = configuration;

            if (FilterEventModel == null)
                FilterEventModel = new FilterEventModel()
                {
                    EventTypeId = (int)EventTypes.All,
                    EventGenreId = 0,
                    ItemsOnPage = Int32.Parse(Configuration["EventPageSize"]),
                    CurrentPage = Int32.Parse(Configuration["PageIndexStart"])
                };
        }

        public async Task CreateEvent(CreateEventModel model)
        {
            var createModel = CreateEventValidator.Validate(model);

            if (!createModel.IsValid)
            {
                model.EventTypes = await InitEventTypeValues();
                model.ArtistOptions = await InitArtists();
                model.Locations = await InitLocations();
                createModel.ThenThrow(model);
            }

            var listOfArtists = new List<Artist>();

            if (model.ArtistsIds != null)
            {
                foreach (var artistId in model.ArtistsIds)
                {
                    var artistToAdd = await UnitOfWork.Artists.Get()
                            .SingleOrDefaultAsync(a => a.Id == artistId);
                    if (artistToAdd != null)
                    {
                        listOfArtists.Add(artistToAdd);
                    }
                }
            }

            var newEvent = Mapper.Map<CreateEventModel, Event>(model);

            if (model.Picture != null)
            {
                var picture = new Picture();
                picture.Id = Guid.NewGuid();
                using (var ms = new MemoryStream())
                {
                    model.Picture.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    picture.PictureContent = fileBytes;
                }

                newEvent.Picture = picture;
            }

            newEvent.Artists = listOfArtists;

            newEvent.OrganizerId = CurrentUser.Id;

            UnitOfWork.Events.Insert(newEvent);
            UnitOfWork.SaveChanges();
        }

        public async Task EditEvent(EditEventModel model)
        {
            var editModel = EditEventValidator.Validate(model);

            var editedEvent = await UnitOfWork.Events
                .Get()
                .Include(e => e.Artists)
                .Include(p => p.Picture)
                .SingleOrDefaultAsync(e => e.Id == model.Id);

            if (!editModel.IsValid)
            {
                model.EventTypes = await InitEventTypeValues();
                model.ArtistOptions = await InitArtists(model.ArtistsIds);
                model.Locations = await InitLocations();
                if (editedEvent.Picture != null)
                {
                    model.OldPicture = editedEvent.Picture.PictureContent;
                }
                editModel.ThenThrow(model);
            }


            var newListOfArtists = new List<Artist>();

            if (editedEvent == null)
            {
                throw new NotFoundErrorException();
            }

            if (model.ArtistsIds != null)
            {
                newListOfArtists.AddRange(await UnitOfWork.Artists.Get()
                    .Where(a => model.ArtistsIds.Contains(a.Id))
                    .ToListAsync());
            }

            var oldPicture = await UnitOfWork.Pictures.Get().SingleOrDefaultAsync(p => p.Id == model.PictureId);

            ExecuteInTransaction(uow =>
            {
                Mapper.Map<EditEventModel, Event>(model, editedEvent);


                if (model.NewPicture != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        model.NewPicture.CopyTo(ms);
                        var fileBytes = ms.ToArray();

                        if (oldPicture == null)
                        {
                            oldPicture = new Picture()
                            {
                                Id = Guid.NewGuid()
                            };
                        }

                        oldPicture.PictureContent = fileBytes;
                        editedEvent.Picture = oldPicture;
                    }
                }
              
                editedEvent.Artists = newListOfArtists;

                uow.Events.Update(editedEvent);
                uow.SaveChanges();

            });
        }

        public async Task DeleteEvent(Guid Id)
        {
            var eventToDelete = await UnitOfWork.Events
                .Get()
                .Include(e => e.Artists)
                .Include(e => e.Picture)
                .Include(e => e.Tickets)
                .SingleOrDefaultAsync(e => e.Id == Id);

            if (eventToDelete == null)
                throw new NotFoundErrorException();

            var eventPicture = await UnitOfWork.Pictures
                .Get()
                .SingleOrDefaultAsync(p => p.Id == eventToDelete.PictureId);

            ExecuteInTransaction(uow =>
            {
                eventToDelete.Artists.Clear();

                uow.Tickets.DeleteRange(eventToDelete.Tickets);

                uow.Events.Delete(eventToDelete);
                if (eventPicture != null)
                {
                    uow.Pictures.Delete(eventPicture);
                }


                uow.SaveChanges();
            });

        }

        public async Task<List<ListEventsModel>> GetEvents(FilterEventModel? filterEventModel)
        {
            var eventsQuery = UnitOfWork.Events
                .Get()
                .Include(e => e.Organizer)
                .Include(e => e.Picture)
                .Include(e => e.Location)
                .Include(e => e.EventNavigation)
                    .ThenInclude(en => en.EventType)
                .Where(e => e.EndDate >= DateTime.Now
                    && e.Location.Status == (int)StatusCode.Approved
                    && e.Artists.All(a => a.Status == (int)StatusCode.Approved)
                    && e.Organizer.DeletedAt == null);

            if (filterEventModel != null)
            {
                if (filterEventModel.EventTypeId.HasValue)
                {
                    FilterEventModel.EventTypeId = filterEventModel.EventTypeId;
                }
                if (filterEventModel.EventGenreId.HasValue)
                {
                    FilterEventModel.EventGenreId = filterEventModel.EventGenreId;
                }

                if (filterEventModel.StartDate != null)
                {
                    FilterEventModel.StartDate = filterEventModel.StartDate;
                }

                if (filterEventModel.EndDate != null)
                {
                    FilterEventModel.EndDate = filterEventModel.EndDate;
                }

                if (filterEventModel.CurrentPage != 0 && filterEventModel.CurrentPage != 1 && filterEventModel.CurrentPage != -1)
                    FilterEventModel.CurrentPage = 1;
                else
                    FilterEventModel.CurrentPage += filterEventModel.CurrentPage;
            }

            if (FilterEventModel.EventTypeId != 0)
                eventsQuery = eventsQuery.Where(e => e.EventTypeId == FilterEventModel.EventTypeId);
            if (FilterEventModel.EventGenreId != 0)
                eventsQuery = eventsQuery.Where(e => e.EventGenreId == FilterEventModel.EventGenreId);
            if (FilterEventModel.StartDate != null)
                eventsQuery = eventsQuery.Where(e => e.StartDate >= FilterEventModel.StartDate);
            if (FilterEventModel.EndDate != null)
                eventsQuery = eventsQuery.Where(e => e.EndDate <= FilterEventModel.EndDate);



            if (FilterEventModel.CurrentPage < 1)
            {
                FilterEventModel.CurrentPage = 1;
            }
            var numberOfEvents = eventsQuery.Count();

            var elementsToSkip = (FilterEventModel.CurrentPage - 1) * FilterEventModel.ItemsOnPage;

            if (numberOfEvents != 0 && elementsToSkip >= numberOfEvents)
            {
                FilterEventModel.CurrentPage--;
                elementsToSkip = (FilterEventModel.CurrentPage - 1) * FilterEventModel.ItemsOnPage;
            }

            var events = await eventsQuery.OrderBy(e => e.StartDate)
                .Skip(elementsToSkip)
                .Take(FilterEventModel.ItemsOnPage)
                .ToListAsync();

            if (events == null)
                throw new NotFoundErrorException();

            List<ListEventsModel> eventsToListEventsModels = new();
            foreach (var ev in events)
            {
                eventsToListEventsModels.Add(Mapper.Map<Event, ListEventsModel>(ev));
            }

            return eventsToListEventsModels;
        }

        public async Task<List<ListItemModel<string, int>>> GetCorrespondingEventGenres(string eventTypeId)
        {
            var isCorrectId = Int32.TryParse(eventTypeId, out var eventTypeIdAsInt);
            var correctEventGenres = await UnitOfWork.EventTypeGenres.Get()
                .Where(etg => etg.EventTypeId == eventTypeIdAsInt)
                .Include(evG => evG.EventGenre)
                .Select(etg => new ListItemModel<string, int>
                {
                    Value = etg.EventGenreId,
                    Text = etg.EventGenre.Name
                }).ToListAsync();
            return correctEventGenres;
        }

        public async Task<CreateEventModel> CreateEventModelAndInitValuesAsync()
        {
            var createEventModel = new CreateEventModel();
            createEventModel.EventTypes = await InitEventTypeValues();
            createEventModel.Locations = await InitLocations();
            createEventModel.ArtistOptions = await InitArtists();
            return createEventModel;
        }

        private async Task<List<ListItemModel<string, int>>> InitEventTypeValues(bool? option = null)
        {
            var eventTypesList = new List<ListItemModel<string, int>>();

            var eventTypesDb = await UnitOfWork.EventTypes
                .Get()
                .Where(ev => option != null ? ev.IsNightEventType == option : true)
                .ToListAsync();

            foreach (var evType in eventTypesDb)
            {
                eventTypesList.Add(new ListItemModel<string, int>()
                {
                    Text = evType.Name,
                    Value = evType.Id
                });
            };

            return eventTypesList;
        }

        public async Task<List<ListItemModel<string, int>>> GetEventTypeValuesForMap(string option)
        {
            var result = bool.TryParse(option, out var optionToBool);
            if (result == false)
                return null;
            var eventTypes = await InitEventTypeValues(optionToBool);
            return eventTypes;
        }

        private async Task<List<ListItemModel<string, Guid>>> InitLocations()
        {
            var availableLocations = await UnitOfWork.Locations.Get()
                .Select(l => new ListItemModel<string, Guid>()
                {
                    Text = l.Name,
                    Value = l.Id
                }).ToListAsync();
            return availableLocations;
        }

        private async Task<List<MultiSelectListItemModel<string, Guid>>> InitArtists(List<Guid> selectedValues = null)
        {
            var availableArtists = await UnitOfWork.Artists.Get()
                .Select(l => new MultiSelectListItemModel<string, Guid>()
                {
                    text = l.Name,
                    value = l.Id,
                    selected = false
                }).ToListAsync();

            if (selectedValues != null)
            {
                foreach (var artist in availableArtists)
                {
                    if (selectedValues.Contains(artist.value))
                    {
                        artist.selected = true;
                    }
                }
            }


            return availableArtists;
        }

        public async Task<DetailsEventModel> GetEventForDetails(Guid id)
        {
            var searchedEvent = await UnitOfWork.Events.Get()
                .Include(p => p.Picture)
                .Include(evt => evt.EventNavigation.EventType)
                .Include(evg => evg.EventNavigation.EventGenre)
                .Include(l => l.Location)
                .Include(t => t.Tickets)
                .Include(a => a.Artists)
                    .ThenInclude(p => p.Picture)
                .SingleOrDefaultAsync(e => e.Id == id);

            if (searchedEvent == null)
                throw new NotFoundErrorException();

            var detailsModel = Mapper.Map<Event, DetailsEventModel>(searchedEvent);

            detailsModel.IsMyEvent = CurrentUser.Id == searchedEvent.OrganizerId;

            return detailsModel;
        }

        public async Task<EditEventModel> GetEventForEdit(Guid id)
        {
            var searchedEvent = await UnitOfWork.Events.Get()
                .Include(p => p.Picture)
                .Include(l => l.Location)
                .Include(a => a.Artists)
                .SingleOrDefaultAsync(e => e.Id == id);

            if (searchedEvent == null)
                throw new NotFoundErrorException();

            if (CurrentUser.Role != RoleTypes.Admin.ToString() && searchedEvent.OrganizerId != CurrentUser.Id)
                throw new AccessViolationException();

            var editEvent = Mapper.Map<Event, EditEventModel>(searchedEvent);

            editEvent.EventTypes = await InitEventTypeValues();
            editEvent.ArtistOptions = await InitArtists(editEvent.ArtistsIds);
            editEvent.Locations = await InitLocations();

            return editEvent;
        }

        public FilterEventModel GetFiltersAndCurrentPage()
        {
            return FilterEventModel;
        }

        public async Task<string> CreatePromptForChat(string request)
        {
            var user = await UnitOfWork.Users
                .Get()
                .SingleOrDefaultAsync(u => u.Id == CurrentUser.Id);

            var events = await UnitOfWork.Events
                .Get()
                .Include(e => e.EventNavigation)
                    .ThenInclude(en => en.EventGenre)
                .Where(e => e.StartDate >= DateTime.Now)
                .ToListAsync();

            var context = "I have a list of events [EventName : Genre]:\n";
            foreach (var ev in events)
            {
                context += "[" + ev.Name + " : " + ev.EventNavigation.EventGenre.Name + "]" + "\n";
            }

            if (user.FavouriteGenre != null)
            {
                var favouriteGenreName = await UnitOfWork.EventGenres
                    .Get()
                    .Where(e => e.Id == user.FavouriteGenre)
                    .SingleOrDefaultAsync();
                context += "My favourite genre is" + favouriteGenreName.Name + ".";
            }

            context += request;
            context += "From the list i gave you.";
            var result = GetResult(context);
            return result;
        }

        public string GetResult(string prompt)
        {
            string apikey = Configuration["gptApiKey"];
            string answer = string.Empty;
            var openai = new OpenAIAPI(apikey);
            CompletionRequest completion = new CompletionRequest();
            completion.Prompt = prompt;
            completion.Model = OpenAI_API.Models.Model.DavinciText;
            completion.MaxTokens = 200;
            var res = openai.Completions.CreateCompletionAsync(completion);
            if (res != null)
            {
                foreach (var item in res.Result.Completions)
                {
                    answer = item.Text;
                }
                return answer;
            }
            else
            {
                throw new NotFoundErrorException();
            }
        }

        public async Task<List<StatisticEventModel>> GetOrganizerEvents()
        {
            var events = await UnitOfWork.Events
                .Get()
                .Include(e => e.Picture)
                .Include(e => e.Location)
                .Include(e => e.Tickets)
                    .ThenInclude(t => t.TicketTransactions)
                        .ThenInclude(tt => tt.Transaction)
                .Where(e => e.OrganizerId == CurrentUser.Id)
                .Select(e => Mapper.Map<StatisticEventModel>(e))
                .ToListAsync();

            events = events.OrderByDescending(e => e.NoSales).ToList();
            return events;
        }

        public async Task<object> GetMonthlyTransactions()
        {
            var user = await UnitOfWork.Users
                .Get()
                .Include(u => u.Events)
                    .ThenInclude(e => e.Tickets)
                        .ThenInclude(t => t.TicketTransactions)
                            .ThenInclude(tt => tt.Transaction)
                .SingleOrDefaultAsync(u => u.Id == CurrentUser.Id);
            var allMonths = Enumerable.Range(1, 12);

            var monthlyTicketTransactionCounts = allMonths
                .GroupJoin(
                    user.Events
                        .SelectMany(e => e.Tickets)
                        .SelectMany(t => t.TicketTransactions)
                        .Where(tt => tt.Transaction.ProcessingDate != null && tt.Transaction.ProcessingDate.Value.Year == DateTime.Now.Year)
                        .GroupBy(tt => tt.Transaction.ProcessingDate.Value.Month),
                    month => month,
                    transactionGroup => transactionGroup.Key,
                    (month, transactionGroup) => new SalesStatistics
                    {
                        Name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                        NoTransactions = transactionGroup.FirstOrDefault()?.Count() ?? 0
                    })
                .ToList();

            return monthlyTicketTransactionCounts;
        }

        public async Task<object> GetTicketTransactionsByEventType()
        {
            var user = await UnitOfWork.Users
                .Get()
                .Include(u => u.Events)
                    .ThenInclude(e => e.Tickets)
                        .ThenInclude(t => t.TicketTransactions)
                            .ThenInclude(tt => tt.Transaction)
                .SingleOrDefaultAsync(u => u.Id == CurrentUser.Id);

            var ticketTransactionsByEventType = user.Events
                .SelectMany(e => e.Tickets)
                .SelectMany(t => t.TicketTransactions)
                .Where(tt => tt.Transaction.ProcessingDate != null && tt.Transaction.ProcessingDate.Value.Year == DateTime.Now.Year)
                .GroupBy(tt => tt.Ticket.Event.EventTypeId)
                .Select(group => new SalesStatistics
                {
                    Name = Enum.GetName(typeof(EventTypes), group.Key),
                    NoTransactions = group.Count()
                })
                .ToList();

            return ticketTransactionsByEventType;
        }
    }
}
