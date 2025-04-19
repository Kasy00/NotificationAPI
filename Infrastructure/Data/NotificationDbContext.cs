using Microsoft.EntityFrameworkCore;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    { }

    public NotificationDbContext() { }

    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationDeliveryAttempt> NotificationDeliveryAttempts { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            
            var connectionString = configuration.GetConnectionString("DefaultConnetion");
            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity => {
            entity.HasKey(n => n.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Channel).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.ToTable("notifications");
        });

        modelBuilder.Entity<NotificationDeliveryAttempt>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("notification_delivery_attempts");
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("outbox_messages");
        });
    }
}