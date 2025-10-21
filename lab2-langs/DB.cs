using Microsoft.EntityFrameworkCore;

namespace WeatherJournalApp
{
    public class Weather : DbContext
    {
        public DbSet<WeatherRec> Records { get; set; }


        public Weather()
        {
        }


        public Weather(DbContextOptions<Weather> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeatherRec>().ToTable("WeatherRecords");

            modelBuilder.Entity<WeatherRec>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<WeatherRec>()
                .Property(r => r.Type)
                .HasMaxLength(40);
        }
    }
}