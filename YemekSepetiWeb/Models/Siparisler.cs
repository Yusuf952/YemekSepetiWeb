using System;
using System.Collections.Generic;

namespace YemekSepetiWeb.Models;

public partial class Siparisler
{
    public int SiparisId { get; set; }

    public int? KullaniciId { get; set; }

    public int? RestoranId { get; set; }

    public DateTime? Tarih { get; set; }

    public decimal? ToplamTutar { get; set; }

    public string? Durum { get; set; }

    public virtual Kullanicilar? Kullanici { get; set; }

    public virtual Restoranlar? Restoran { get; set; }

    public virtual ICollection<SiparisDetay> SiparisDetays { get; set; } = new List<SiparisDetay>();
}
