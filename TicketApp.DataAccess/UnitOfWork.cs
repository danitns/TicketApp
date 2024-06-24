using TicketApp.Common;
using TicketApp.DataAccess;
using TicketApp.Entities;

namespace TicketApp.DataAccess
{
    public class UnitOfWork
    {
        private readonly TicketAppContext Context;

        public UnitOfWork(TicketAppContext context)
        {
            this.Context = context;
        }

        private IRepository<Artist> artists;
        public IRepository<Artist> Artists => artists ?? (artists = new BaseRepository<Artist>(Context));


        private IRepository<ArtistType> artistTypes;
        public IRepository<ArtistType> ArtistTypes => artistTypes ?? (artistTypes = new BaseRepository<ArtistType>(Context));


        private IRepository<Event> events;
        public IRepository<Event> Events => events ?? (events = new BaseRepository<Event>(Context));


        private IRepository<EventGenre> eventGenres;
        public IRepository<EventGenre> EventGenres => eventGenres ?? (eventGenres = new BaseRepository<EventGenre>(Context));


        private IRepository<EventType> eventTypes;
        public IRepository<EventType> EventTypes => eventTypes ?? (eventTypes = new BaseRepository<EventType>(Context));


        private IRepository<EventTypeGenre> eventTypeGenres;
        public IRepository<EventTypeGenre> EventTypeGenres => eventTypeGenres ?? (eventTypeGenres = new BaseRepository<EventTypeGenre>(Context));


        private IRepository<Location> locations;
        public IRepository<Location> Locations => locations ?? (locations = new BaseRepository<Location>(Context));


        private IRepository<Picture> pictures;
        public IRepository<Picture> Pictures => pictures ?? (pictures = new BaseRepository<Picture>(Context));


        //private IRepository<Product> products;
        //public IRepository<Product> Products => products ?? (products = new BaseRepository<Product>(Context));


        //private IRepository<Product> productTransactions;
        //public IRepository<Product> ProductTransactions => productTransactions ?? (productTransactions = new BaseRepository<Product>(Context));


        private IRepository<Role> roles;
        public IRepository<Role> Roles => roles ?? (roles = new BaseRepository<Role>(Context));


        private IRepository<Subscription> subscriptions;
        public IRepository<Subscription> Subscriptions => subscriptions ?? (subscriptions = new BaseRepository<Subscription>(Context));


        private IRepository<Ticket> tickets;
        public IRepository<Ticket> Tickets => tickets ?? (tickets = new BaseRepository<Ticket>(Context));


        private IRepository<TicketTransaction> ticketTransactions;
        public IRepository<TicketTransaction> TicketTransactions => ticketTransactions ?? (ticketTransactions = new BaseRepository<TicketTransaction>(Context));


        private IRepository<Transaction> transactions;
        public IRepository<Transaction> Transactions => transactions ?? (transactions = new BaseRepository<Transaction>(Context));


        private IRepository<User> users;
        public IRepository<User> Users => users ?? (users = new BaseRepository<User>(Context));


        public void SaveChanges()
        {
            Context.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await Context.SaveChangesAsync();
        }
    }
}
