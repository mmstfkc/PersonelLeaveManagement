using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PersonelLeaveManagement.Domain.Entities;

namespace PersonelLeaveManagement.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Personel> Personeller { get; set; }
    public DbSet<IzinTalebi> IzinTalepleri { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Personel>(entity =>
            {
                entity.ToTable("Personeller");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Ad).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Soyad).IsRequired().HasMaxLength(100);
                entity.Property(p => p.TCKimlikNo).IsRequired().HasMaxLength(11);

                entity.HasMany(p => p.IzinTalepleri)
                      .WithOne(i => i.Personel)
                      .HasForeignKey(i => i.PersonelId);
            }
        );

        modelBuilder.Entity<IzinTalebi>(entity =>
            {
                entity.ToTable("IzinTalepleri");
                entity.HasKey(i => i.Id);

                entity.Property(i => i.Durum)
                .IsRequired()
                .HasMaxLength(100);
            }
        );
    }
}