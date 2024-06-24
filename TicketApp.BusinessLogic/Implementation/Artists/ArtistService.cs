using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Common.DTOs;
using TicketApp.Common.Exceptions;
using TicketApp.Common.Extensions;
using TicketApp.Entities;
using TicketApp.Entities.Enums;

namespace TicketApp.BusinessLogic.Implementation.Artists
{
    public class ArtistService : BaseService
    {
        private readonly CreateArtistValidator CreateArtistValidator;
        public ArtistService(ServiceDependencies serviceDependencies) : base(serviceDependencies)
        {
            CreateArtistValidator = new CreateArtistValidator();
        }

        public Guid CreateArtist(CreateArtistModel model)
        {
            CreateArtistValidator.Validate(model).ThenThrow(model);

            var newArtist = Mapper.Map<CreateArtistModel, Artist>(model);

            if (model.Picture != null)
            {
                var picture = new Picture()
                {
                    Id = Guid.NewGuid()
                };
                using (var ms = new MemoryStream())
                {
                    model.Picture.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    picture.PictureContent = fileBytes;
                }

                newArtist.Picture = picture;
            }

            UnitOfWork.Artists.Insert(newArtist);
            UnitOfWork.SaveChanges();

            return newArtist.Id;
        }

        public async Task<List<DisplayArtistForIndexModel>> GetArtists()
        {
            var artists = await UnitOfWork.Artists
                .Get()
                .Include(a => a.ArtistType)
                .ToListAsync();
            var artistsForDisplay = new List<DisplayArtistForIndexModel>();
            foreach (var artist in artists)
            {
                artistsForDisplay.Add(Mapper.Map<Artist, DisplayArtistForIndexModel>(artist));
            }
            return artistsForDisplay;
        }

        public async Task DeleteArtist(Guid id)
        {
            var artistToDelete = await UnitOfWork.Artists
                .Get()
                .Include(a => a.Events)
                .SingleOrDefaultAsync(a => a.Id == id);

            if (artistToDelete == null)
                throw new NotFoundErrorException();

            artistToDelete.Events.Clear();

            UnitOfWork.Artists.Delete(artistToDelete);
            UnitOfWork.SaveChanges();
        }
    }
}
