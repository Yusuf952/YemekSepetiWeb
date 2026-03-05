using System;
using System.Collections.Generic;

namespace YemekSepetiWeb.Models;

public partial class Kullanicilar
{
    public int KullaniciId { get; set; }

    public string AdSoyad { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Sifre { get; set; } = null!;

    public string? Adres { get; set; }

    public string? Telefon { get; set; }

    public string? Rol { get; set; }

    public virtual ICollection<Siparisler> Siparislers { get; set; } = new List<Siparisler>();
}
