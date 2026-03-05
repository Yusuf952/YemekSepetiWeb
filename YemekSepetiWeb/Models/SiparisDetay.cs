using System;
using System.Collections.Generic;

namespace YemekSepetiWeb.Models;

public partial class SiparisDetay
{
    public int DetayId { get; set; }

    public int? SiparisId { get; set; }

    public int? UrunId { get; set; }

    public int? Adet { get; set; }

    public decimal? BirimFiyat { get; set; }

    public virtual Siparisler? Siparis { get; set; }

    public virtual Urunler? Urun { get; set; }
}
