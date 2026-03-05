using System;
using System.Collections.Generic;

namespace YemekSepetiWeb.Models;

public partial class ViewSiparisOzet
{
    public int SiparisId { get; set; }

    public string Musteri { get; set; } = null!;

    public string RestoranAdi { get; set; } = null!;

    public DateTime? Tarih { get; set; }

    public decimal? ToplamTutar { get; set; }

    public string? Durum { get; set; }
}
