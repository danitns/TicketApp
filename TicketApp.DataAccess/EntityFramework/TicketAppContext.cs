using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TicketApp.Entities;

namespace TicketApp.DataAccess;

public partial class TicketAppContext : DbContext
{
    public TicketAppContext()
    {
    }

    public TicketAppContext(DbContextOptions<TicketAppContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Artist> Artists { get; set; }

    public virtual DbSet<ArtistType> ArtistTypes { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventGenre> EventGenres { get; set; }

    public virtual DbSet<EventType> EventTypes { get; set; }

    public virtual DbSet<EventTypeGenre> EventTypeGenres { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<LocationType> LocationTypes { get; set; }

    public virtual DbSet<Picture> Pictures { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketTransaction> TicketTransactions { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(local);Initial Catalog=TicketApp;Integrated Security=true;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Artist__3214EC07A2B80FA2");

            entity.ToTable("Artist");

            entity.HasIndex(e => e.Name, "UQ_ARTIST_NAME").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Birthdate).HasColumnType("date");
            entity.Property(e => e.Debut).HasColumnType("date");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Status).HasDefaultValueSql("((2))");

            entity.HasOne(d => d.ArtistType).WithMany(p => p.Artists)
                .HasForeignKey(d => d.ArtistTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ARTIST_ARTISTTYPE");

            entity.HasOne(d => d.Picture).WithMany(p => p.Artists)
                .HasForeignKey(d => d.PictureId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ARTIST_PICTURE");
        });

        modelBuilder.Entity<ArtistType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ArtistTy__3214EC0781100ED1");

            entity.ToTable("ArtistType");

            entity.HasIndex(e => e.Name, "UQ_ARTISTTYPE_NAME").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Event__3214EC07B04B1498");

            entity.ToTable("Event");

            entity.HasIndex(e => e.Name, "UQ_EVENT_NAME").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.Location).WithMany(p => p.Events)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EVENT_LOCATION");

            entity.HasOne(d => d.Organizer).WithMany(p => p.Events)
                .HasForeignKey(d => d.OrganizerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EVENT_USER");

            entity.HasOne(d => d.Picture).WithMany(p => p.Events)
                .HasForeignKey(d => d.PictureId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EVENT_PICTURE");

            entity.HasOne(d => d.EventNavigation).WithMany(p => p.Events)
                .HasForeignKey(d => new { d.EventTypeId, d.EventGenreId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EVENT_EVENT_TYPE_GENRE");

            entity.HasMany(d => d.Artists).WithMany(p => p.Events)
                .UsingEntity<Dictionary<string, object>>(
                    "EventArtist",
                    r => r.HasOne<Artist>().WithMany()
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_EVENT_ARTIST_ARTIST"),
                    l => l.HasOne<Event>().WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_EVENT_ARTIST_EVENT"),
                    j =>
                    {
                        j.HasKey("EventId", "ArtistId").HasName("PK_EVENT_ARTIST");
                        j.ToTable("Event_Artist");
                    });
        });

        modelBuilder.Entity<EventGenre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EventGen__3214EC075D704958");

            entity.ToTable("EventGenre");

            entity.HasIndex(e => e.Name, "UQ_EVGENRE_NAME").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(40);
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EventTyp__3214EC0723884F7A");

            entity.ToTable("EventType");

            entity.HasIndex(e => e.Name, "UQ_EVENTTYPE_NAME").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(40);
        });

        modelBuilder.Entity<EventTypeGenre>(entity =>
        {
            entity.HasKey(e => new { e.EventTypeId, e.EventGenreId }).HasName("PK_EVENT_TYPE_GENRE");

            entity.ToTable("Event_Type_Genre");

            entity.HasOne(d => d.EventGenre).WithMany(p => p.EventTypeGenres)
                .HasForeignKey(d => d.EventGenreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EVENT_TYPE_GENRE_GENRE");

            entity.HasOne(d => d.EventType).WithMany(p => p.EventTypeGenres)
                .HasForeignKey(d => d.EventTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EVENT_TYPE_GENRE_TYPE");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Location__3214EC072155AE35");

            entity.ToTable("Location");

            entity.HasIndex(e => new { e.Name, e.Address }, "UQ_LOCATION_NAME_ADDRESS").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Latitude).HasColumnType("decimal(10, 8)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(11, 8)");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Status).HasDefaultValueSql("((2))");

            entity.HasOne(d => d.LocationType).WithMany(p => p.Locations)
                .HasForeignKey(d => d.LocationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LOCATION_LOCATIONTYPE");
        });

        modelBuilder.Entity<LocationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Location__3214EC077125B4E2");

            entity.ToTable("LocationType");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Picture>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_IMAGINE");

            entity.ToTable("Picture");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Role__3214EC07815DAC6C");

            entity.ToTable("Role");

            entity.HasIndex(e => e.Name, "UQ_ROLE_NAME").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(40);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subscrip__3214EC07E36F3629");

            entity.ToTable("Subscription");

            entity.HasIndex(e => e.Name, "UQ_SUBSCRIPTION_NAME").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("money");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TICKET");

            entity.ToTable("Ticket");

            entity.HasIndex(e => new { e.Name, e.EventId }, "UQ_TICKET_NAME_EVENTID").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Notickets).HasColumnName("NOTickets");
            entity.Property(e => e.Price).HasColumnType("money");

            entity.HasOne(d => d.Event).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TICKET_EVENT");
        });

        modelBuilder.Entity<TicketTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TICKET_TRANSACTION");

            entity.ToTable("Ticket_Transaction");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketTransactions)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK_TICKET_TRANSACTION_TICKET");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TicketTransactions)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_TICKET_TRANSACTION_TRANSACTION");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC0740C86F67");

            entity.ToTable("Transaction");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.IsFree)
                .IsRequired()
                .HasDefaultValueSql("('false')");
            entity.Property(e => e.ProcessingDate).HasColumnType("datetime");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("FK_TRANSACTION_SUBSCRIPTION");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TRANSACTION_USER");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC073E6907E6");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ_USER_EMAIL").IsUnique();

            entity.HasIndex(e => e.Phone, "UQ_UTILIZATOR_PHONE").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Birthdate).HasColumnType("date");
            entity.Property(e => e.DeletedAt).HasColumnType("date");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(15);

            entity.HasOne(d => d.FavouriteGenreNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.FavouriteGenre)
                .HasConstraintName("FK_USER_FAVOURITEGENRE");

            entity.HasOne(d => d.Picture).WithMany(p => p.Users)
                .HasForeignKey(d => d.PictureId)
                .HasConstraintName("FK_USER_PICTURE");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_ROLE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
