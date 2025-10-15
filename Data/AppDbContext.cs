
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Hall> Halls { get; set; }
    public DbSet<Spectacle> Spectacles { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<UserVisit> UserVisits { get; set; }
    public DbSet<ActionHistory> ActionHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<User>()
            .HasOne(u => u.role)
            .WithMany()
            .HasForeignKey(u => u.role_id);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.spectacle)
            .WithMany()
            .HasForeignKey(t => t.spectacle_id);

        modelBuilder.Entity<Spectacle>()
        .HasOne(s => s.hall)
        .WithMany()
        .HasForeignKey(s => s.hall_id);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.user)
            .WithMany()
            .HasForeignKey(t => t.user_id);

        modelBuilder.Entity<UserVisit>()
            .HasOne(uv => uv.user)
            .WithMany()
            .HasForeignKey(uv => uv.user_id);

        modelBuilder.Entity<UserVisit>()
            .HasOne(uv => uv.spectacle)
            .WithMany()
            .HasForeignKey(uv => uv.spectacle_id);

        
        modelBuilder.Entity<ActionHistory>()
            .HasOne(ah => ah.user)
            .WithMany()
            .HasForeignKey(ah => ah.user_id)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ActionHistory>()
            .Property(ah => ah.old_value)
            .IsRequired(false);

        modelBuilder.Entity<ActionHistory>()
            .Property(ah => ah.new_value)
            .IsRequired(false);

        modelBuilder.Entity<ActionHistory>()
            .Property(ah => ah.metadata)
            .IsRequired(false);

        
        modelBuilder.Entity<User>()
            .HasCheckConstraint("ck_user_blocked_until", "(is_blocked = false AND blocked_until IS NULL) OR (is_blocked = true AND blocked_until IS NOT NULL)");

        modelBuilder.Entity<Ticket>()
            .HasCheckConstraint("ck_ticket_reserved_until", "(status != 'reserved' AND reserved_until IS NULL) OR (status = 'reserved' AND reserved_until IS NOT NULL)");

        // сохраняем DateTime как UTC
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
        );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }
    }
}
