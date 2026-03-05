using System;
using System.Collections.Generic;

namespace YemekSepetiWeb.Models;

public partial class Urunler
{
    public int UrunId { get; set; }

    public int? RestoranId { get; set; }

    public string UrunAdi { get; set; } = null!;

    public string? Aciklama { get; set; }

    public decimal Fiyat { get; set; }

    public virtual Restoranlar? Restoran { get; set; }

    public virtual ICollection<SiparisDetay> SiparisDetays { get; set; } = new List<SiparisDetay>();
}
