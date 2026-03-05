using System;
using System.Collections.Generic;

namespace YemekSepetiWeb.Models;

public partial class Restoranlar
{
    public int RestoranId { get; set; }

    public string RestoranAdi { get; set; } = null!;

    public string? Kategori { get; set; }

    public decimal? MinTutar { get; set; }

    public decimal? Puan { get; set; }

    public string? ResimYolu { get; set; }

    public virtual ICollection<Siparisler> Siparislers { get; set; } = new List<Siparisler>();

    public virtual ICollection<Urunler> Urunlers { get; set; } = new List<Urunler>();
}
