using BourbonVault.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BourbonVault.Data
{
    public class BourbonVaultContext : IdentityDbContext<ApplicationUser>
    {
        public BourbonVaultContext(DbContextOptions<BourbonVaultContext> options)
            : base(options)
        {
        }

        public DbSet<Bottle> Bottles { get; set; }
        public DbSet<Distillery> Distilleries { get; set; }
        public DbSet<TastingNote> TastingNotes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BottleTag> BottleTags { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Important for Identity tables
            
            // Configure Identity tables
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "AspNetUsers");
            });
            base.OnModelCreating(modelBuilder);

            // Configure Bottle entity
            modelBuilder.Entity<Bottle>()
                .HasOne(b => b.Distillery)
                .WithMany(d => d.Bottles)
                .HasForeignKey(b => b.DistilleryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TastingNote entity
            modelBuilder.Entity<TastingNote>()
                .HasOne(tn => tn.Bottle)
                .WithMany(b => b.TastingNotes)
                .HasForeignKey(tn => tn.BottleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure BottleTag (many-to-many relationship)
            modelBuilder.Entity<BottleTag>()
                .HasOne(bt => bt.Bottle)
                .WithMany(b => b.BottleTags)
                .HasForeignKey(bt => bt.BottleId);

            modelBuilder.Entity<BottleTag>()
                .HasOne(bt => bt.Tag)
                .WithMany(t => t.BottleTags)
                .HasForeignKey(bt => bt.TagId);

            // Configure indexes for better query performance
            modelBuilder.Entity<Bottle>()
                .HasIndex(b => b.UserId);

            modelBuilder.Entity<Bottle>()
                .HasIndex(b => b.DistilleryId);

            modelBuilder.Entity<TastingNote>()
                .HasIndex(tn => tn.UserId);

            modelBuilder.Entity<TastingNote>()
                .HasIndex(tn => tn.BottleId);
        }
    }
}
