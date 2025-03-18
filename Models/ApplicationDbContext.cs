using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MyBGList.Models;

public class ApplicationDbContext : IdentityDbContext<ApiUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<BoardGame>? BoardGames => Set<BoardGame>();
    public DbSet<Domain>? Domains => Set<Domain>();
    public DbSet<Mechanic>? Mechanics => Set<Mechanic>();

    public DbSet<BoardGames_Domains>? BoardGames_Domains => Set<BoardGames_Domains>();
    public DbSet<BoardGames_Mechanics>? BoardGames_Mechanics => Set<BoardGames_Mechanics>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BoardGames_Domains>()
            .HasKey(i => new { i.BoardGameId, i.DomainId });

        modelBuilder.Entity<BoardGames_Domains>()
            .HasOne(x => x.BoardGame)
            .WithMany(y => y.BoardGames_Domains)
            .HasForeignKey(f => f.BoardGameId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BoardGames_Domains>()
            .HasOne(o => o.Domain)
            .WithMany(m => m.BoardGames_Domains)
            .HasForeignKey(f => f.DomainId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BoardGames_Mechanics>()
            .HasKey(i => new { i.BoardGameId, i.MechanicId });

        modelBuilder.Entity<BoardGames_Mechanics>()
            .HasOne(x => x.BoardGame)
            .WithMany(y => y.BoardGames_Mechanics)
            .HasForeignKey(f => f.BoardGameId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BoardGames_Mechanics>()
            .HasOne(o => o.Mechanic)
            .WithMany(m => m.BoardGames_Mechanics)
            .HasForeignKey(f => f.MechanicId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        //change default identity table names
        /*modelBuilder.Entity<ApiUser>().ToTable("ApiUsers");
        modelBuilder.Entity<IdentityRole<string>>().ToTable("ApiRoles");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("ApiRoleClaims");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("ApiUserClaims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("ApiUserLogins");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("ApiToRoles");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("ApiUserTokens");*/
    }
}