using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace ELeaguesServer.Models;

public partial class KrzmauContext : DbContext
{
    public KrzmauContext()
    {
    }

    public KrzmauContext(DbContextOptions<KrzmauContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ligi> Ligis { get; set; }

    public virtual DbSet<Mecze> Meczes { get; set; }

    public virtual DbSet<Turnieje> Turniejes { get; set; }

    public virtual DbSet<Uzytkownicy> Uzytkownicies { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(ConfigurationManager.ConnectionStrings[0].ConnectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ligi>(entity =>
        {
            entity.HasKey(e => e.Idligi).HasName("ligi_pkey");

            entity.ToTable("ligi");

            entity.Property(e => e.Idligi).HasColumnName("idligi");
            entity.Property(e => e.Idwlasciciela).HasColumnName("idwlasciciela");

            entity.HasOne(d => d.IdwlascicielaNavigation).WithMany(p => p.Ligis)
                .HasForeignKey(d => d.Idwlasciciela)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ligi_idwlasciciela_fkey");
        });

        modelBuilder.Entity<Mecze>(entity =>
        {
            entity.HasKey(e => e.Idmeczu).HasName("mecze_pkey");

            entity.ToTable("mecze");

            entity.Property(e => e.Idmeczu).HasColumnName("idmeczu");
            entity.Property(e => e.Idnastepnegomeczu).HasColumnName("idnastepnegomeczu");
            entity.Property(e => e.Idturnieju).HasColumnName("idturnieju");
            entity.Property(e => e.Idzawodnikadwa).HasColumnName("idzawodnikadwa");
            entity.Property(e => e.Idzawodnikajeden).HasColumnName("idzawodnikajeden");
            entity.Property(e => e.Wynikdwa).HasColumnName("wynikdwa");
            entity.Property(e => e.Wynikjeden).HasColumnName("wynikjeden");

            entity.HasOne(d => d.IdnastepnegomeczuNavigation).WithMany(p => p.InverseIdnastepnegomeczuNavigation)
                .HasForeignKey(d => d.Idnastepnegomeczu)
                .HasConstraintName("mecze_idnastepnegomeczu_fkey");

            entity.HasOne(d => d.IdturniejuNavigation).WithMany(p => p.Meczes)
                .HasForeignKey(d => d.Idturnieju)
                .HasConstraintName("mecze_idturnieju_fkey");

            entity.HasOne(d => d.IdzawodnikadwaNavigation).WithMany(p => p.MeczeIdzawodnikadwaNavigations)
                .HasForeignKey(d => d.Idzawodnikadwa)
                .HasConstraintName("mecze_idzawodnikadwa_fkey");

            entity.HasOne(d => d.IdzawodnikajedenNavigation).WithMany(p => p.MeczeIdzawodnikajedenNavigations)
                .HasForeignKey(d => d.Idzawodnikajeden)
                .HasConstraintName("mecze_idzawodnikajeden_fkey");
        });

        modelBuilder.Entity<Turnieje>(entity =>
        {
            entity.HasKey(e => e.Idturnieju).HasName("turnieje_pkey");

            entity.ToTable("turnieje");

            entity.Property(e => e.Idturnieju).HasColumnName("idturnieju");
            entity.Property(e => e.Idligi).HasColumnName("idligi");
            entity.Property(e => e.Liczbarund).HasColumnName("liczbarund");

            entity.HasOne(d => d.IdligiNavigation).WithMany(p => p.Turniejes)
                .HasForeignKey(d => d.Idligi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("turnieje_idligi_fkey");
        });

        modelBuilder.Entity<Uzytkownicy>(entity =>
        {
            entity.HasKey(e => e.Iduzytkownika).HasName("uzytkownicy_pkey");

            entity.ToTable("uzytkownicy");

            entity.HasIndex(e => e.Nazwa, "uzytkownicy_nazwa_key").IsUnique();

            entity.Property(e => e.Iduzytkownika).HasColumnName("iduzytkownika");
            entity.Property(e => e.Administrator).HasColumnName("administrator");
            entity.Property(e => e.Haslo)
                .HasMaxLength(50)
                .HasColumnName("haslo");
            entity.Property(e => e.Nazwa)
                .HasMaxLength(50)
                .HasColumnName("nazwa");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
