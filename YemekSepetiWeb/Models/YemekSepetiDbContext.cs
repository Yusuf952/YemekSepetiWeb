using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace YemekSepetiWeb.Models;

public partial class YemekSepetiDbContext : DbContext
{
    public YemekSepetiDbContext()
    {
    }

    public YemekSepetiDbContext(DbContextOptions<YemekSepetiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Kullanicilar> Kullanicilars { get; set; }

    public virtual DbSet<Restoranlar> Restoranlars { get; set; }

    public virtual DbSet<SiparisDetay> SiparisDetays { get; set; }

    public virtual DbSet<Siparisler> Siparislers { get; set; }

    public virtual DbSet<Urunler> Urunlers { get; set; }

    public virtual DbSet<ViewSiparisOzet> ViewSiparisOzets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=YemekSepetiDb;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kullanicilar>(entity =>
        {
            entity.HasKey(e => e.KullaniciId).HasName("PK__Kullanic__E011F09B706F00FF");

            entity.ToTable("Kullanicilar", tb => tb.HasTrigger("TRG_BuyukHarfAd"));

            entity.HasIndex(e => e.Email, "UQ__Kullanic__A9D10534C8F938CE").IsUnique();

            entity.Property(e => e.KullaniciId).HasColumnName("KullaniciID");
            entity.Property(e => e.AdSoyad).HasMaxLength(100);
            entity.Property(e => e.Adres).HasMaxLength(250);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .HasDefaultValue("Musteri");
            entity.Property(e => e.Sifre).HasMaxLength(50);
            entity.Property(e => e.Telefon).HasMaxLength(15);
        });

        modelBuilder.Entity<Restoranlar>(entity =>
        {
            entity.HasKey(e => e.RestoranId).HasName("PK__Restoran__259AB1A7745C6DA3");

            entity.ToTable("Restoranlar", tb => tb.HasTrigger("TRG_PuanKontrol"));

            entity.Property(e => e.RestoranId).HasColumnName("RestoranID");
            entity.Property(e => e.Kategori).HasMaxLength(50);
            entity.Property(e => e.MinTutar)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Puan)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(3, 1)");
            entity.Property(e => e.ResimYolu).HasMaxLength(250);
            entity.Property(e => e.RestoranAdi).HasMaxLength(100);
        });

        modelBuilder.Entity<SiparisDetay>(entity =>
        {
            entity.HasKey(e => e.DetayId).HasName("PK__SiparisD__8E8164A5F37E85EC");

            entity.ToTable("SiparisDetay");

            entity.Property(e => e.DetayId).HasColumnName("DetayID");
            entity.Property(e => e.Adet).HasDefaultValue(1);
            entity.Property(e => e.BirimFiyat).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SiparisId).HasColumnName("SiparisID");
            entity.Property(e => e.UrunId).HasColumnName("UrunID");

            entity.HasOne(d => d.Siparis).WithMany(p => p.SiparisDetays)
                .HasForeignKey(d => d.SiparisId)
                .HasConstraintName("FK__SiparisDe__Sipar__48CFD27E");

            entity.HasOne(d => d.Urun).WithMany(p => p.SiparisDetays)
                .HasForeignKey(d => d.UrunId)
                .HasConstraintName("FK__SiparisDe__UrunI__49C3F6B7");
        });

        modelBuilder.Entity<Siparisler>(entity =>
        {
            entity.HasKey(e => e.SiparisId).HasName("PK__Siparisl__C3F03BDD86DD8CD4");

            entity.ToTable("Siparisler", tb => tb.HasTrigger("TRG_SiparisSilinemez"));

            entity.Property(e => e.SiparisId).HasColumnName("SiparisID");
            entity.Property(e => e.Durum)
                .HasMaxLength(50)
                .HasDefaultValue("Hazırlanıyor");
            entity.Property(e => e.KullaniciId).HasColumnName("KullaniciID");
            entity.Property(e => e.RestoranId).HasColumnName("RestoranID");
            entity.Property(e => e.Tarih)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ToplamTutar).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Kullanici).WithMany(p => p.Siparislers)
                .HasForeignKey(d => d.KullaniciId)
                .HasConstraintName("FK__Siparisle__Kulla__440B1D61");

            entity.HasOne(d => d.Restoran).WithMany(p => p.Siparislers)
                .HasForeignKey(d => d.RestoranId)
                .HasConstraintName("FK__Siparisle__Resto__44FF419A");
        });

        modelBuilder.Entity<Urunler>(entity =>
        {
            entity.HasKey(e => e.UrunId).HasName("PK__Urunler__623D364BF92115E1");

            entity.ToTable("Urunler", tb => tb.HasTrigger("TRG_FiyatKontrol"));

            entity.Property(e => e.UrunId).HasColumnName("UrunID");
            entity.Property(e => e.Aciklama).HasMaxLength(250);
            entity.Property(e => e.Fiyat).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.RestoranId).HasColumnName("RestoranID");
            entity.Property(e => e.UrunAdi).HasMaxLength(100);

            entity.HasOne(d => d.Restoran).WithMany(p => p.Urunlers)
                .HasForeignKey(d => d.RestoranId)
                .HasConstraintName("FK__Urunler__Restora__3F466844");
        });

        modelBuilder.Entity<ViewSiparisOzet>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VIEW_SiparisOzet");

            entity.Property(e => e.Durum).HasMaxLength(50);
            entity.Property(e => e.Musteri).HasMaxLength(100);
            entity.Property(e => e.RestoranAdi).HasMaxLength(100);
            entity.Property(e => e.SiparisId).HasColumnName("SiparisID");
            entity.Property(e => e.Tarih).HasColumnType("datetime");
            entity.Property(e => e.ToplamTutar).HasColumnType("decimal(10, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
