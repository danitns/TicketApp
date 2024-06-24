using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.Implementation.Account;
using TicketApp.BusinessLogic.Implementation.Artists;
using TicketApp.BusinessLogic.Implementation.Locations;
using TicketApp.Common.Exceptions;
using TicketApp.DataAccess;
using TicketApp.Entities;
using TicketApp.Entities.Enums;

namespace TicketApp.BusinessLogic.Implementation.Admin
{
    public class AdminService : BaseService
    {
        public AdminService(ServiceDependencies serviceDependencies) : base(serviceDependencies)
        {
        }

        public async Task ApproveArtist(Guid artistId)
        {
            var artist = await GetArtist(artistId);
            artist.Status = (int)StatusCode.Approved;
            UnitOfWork.Artists.Update(artist);
            UnitOfWork.SaveChanges();
        }

        public async Task ApproveLocation(Guid locationId)
        {
            var location = await GetLocation(locationId);
            location.Status = (int)StatusCode.Approved;
            UnitOfWork.Locations.Update(location);
            UnitOfWork.SaveChanges();
        }

        public async Task ApproveOrganizer(Guid userId)
        {
            var organizer = await GetUser(userId);
            organizer.RoleId = (int)RoleTypes.Organizer;
            UnitOfWork.Users.Update(organizer);
            UnitOfWork.SaveChanges();
        }

        //public async Task DeleteUser(Guid userId)
        //{
        //    var user = await GetUser(userId);

        //    user.DeletedAt = DateTime.Now;

        //    UnitOfWork.Users.Update(user);
        //    UnitOfWork.SaveChanges();
            
        //}

        public async Task<List<PendingArtistModel>> GetPendingArtists()
        {
            var artists = await UnitOfWork.Artists
                .Get()
                .Where(a => a.Status == (int)StatusCode.Pending)
                .Include(a => a.Picture)
                .Select(a => Mapper.Map<PendingArtistModel>(a))
                .ToListAsync();
            return artists;
        }

        public async Task<List<PendingLocationModel>> GetPendingLocations()
        {
            var locations = await UnitOfWork.Locations
                .Get()
                .Where(a => a.Status == (int)StatusCode.Pending)
                .Select(a => Mapper.Map<PendingLocationModel>(a))
                .ToListAsync();
            return locations;
        }

        public async Task<List<PendingOrganizer>> GetPendingOrganizers()
        {
            var organizers = await UnitOfWork.Users
                .Get()
                .Include(u => u.Picture)
                .Where(u => u.RoleId == (int)RoleTypes.PendingOrganizer)
                .Select(u => Mapper.Map<PendingOrganizer>(u))
                .ToListAsync();
            return organizers;
        }

        public async Task<List<UserDetailsModel>> GetUserList()
        {
            var userList = await UnitOfWork.Users
                .Get()
                .Include(u => u.Picture)
                .Where(u => u.RoleId != (int)RoleTypes.Admin && u.DeletedAt == null)
                .Select(u => Mapper.Map<UserDetailsModel>(u))   
                .ToListAsync();
            return userList;
        }

        public async Task RejectArtist(Guid artistId)
        {
            var artist = await GetArtist(artistId);

            var eventsToDelete = artist.Events;

            artist.Events.Clear();
            UnitOfWork.Artists.Delete(artist);
            UnitOfWork.Events.DeleteRange(eventsToDelete);
            
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task RejectLocation(Guid locationId)
        {
            var location = await GetLocation(locationId);

            var eventsToDelete = location.Events;

            ExecuteInTransaction(uow =>
            {
                foreach(var ev in eventsToDelete)
                {
                    ev.Artists.Clear();
                }
                uow.Events.DeleteRange(eventsToDelete);
                uow.Locations.Delete(location);
                uow.SaveChanges();
            });
        }

        public async Task RejectOrganizer(Guid userId)
        {
            var organizer = await GetUser(userId);
            organizer.RoleId = (int)RoleTypes.User;
            UnitOfWork.Users.Update(organizer);
            UnitOfWork.SaveChanges();
        }

        private async Task<Artist> GetArtist(Guid artistId)
        {
            var artist = await UnitOfWork.Artists
                .Get()
                .Include(a => a.Events)
                .SingleOrDefaultAsync(a => a.Id == artistId);
            if (artist == null)
                throw new NotFoundErrorException();
            return artist;
        }

        private async Task<Location> GetLocation(Guid locationId)
        {
            var location = await UnitOfWork.Locations.Get()
                .Include(l => l.Events)
                    .ThenInclude(e => e.Artists)
                .SingleOrDefaultAsync(a => a.Id == locationId);
            if (location == null)
                throw new NotFoundErrorException();
            return location;
        }

        private async Task<User> GetUser(Guid userId)
        {
            var user = await UnitOfWork.Users.Get()
                .SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new NotFoundErrorException();
            return user;
        }
    }
}
