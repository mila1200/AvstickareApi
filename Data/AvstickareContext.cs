using AvstickareApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AvstickareApi.Data
{
    public class AvstickareContext : DbContext
    {
        public AvstickareContext(DbContextOptions<AvstickareContext> options) : base(options)
        {

        }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripStop> TripStops { get; set; }
        public DbSet<FavoritePlace> FavoritePlaces { get; set; }
        public DbSet<Category> Categories { get; set; }

       protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //säkerställer unik e-postadress
            modelBuilder.Entity<AppUser>()
            .HasIndex(user => user.Email)
            .IsUnique();

            //säkerställer unikt användarnamn
            modelBuilder.Entity<AppUser>()
            .HasIndex(user => user.UserName)
            .IsUnique();
        } 
    }
}