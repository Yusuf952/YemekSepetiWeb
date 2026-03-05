using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList için gerekli
using System.Text.Json;
using YemekSepetiWeb.Models;

namespace YemekSepetiWeb.Controllers
{
    public class MusteriController : Controller
    {
        private readonly YemekSepetiDbContext _context;

        public MusteriController(YemekSepetiDbContext context)
        {
            _context = context;
        }

        // 1. VİTRİN: Tüm Restoranları Listele
        public async Task<IActionResult> Index()
        {
            var restoranlar = await _context.Restoranlars.ToListAsync();
            return View(restoranlar);
        }

        // 2. MENÜ: Seçilen Restoranın Yemekleri
        public async Task<IActionResult> Menu(int? id)
        {
            if (id == null) return RedirectToAction("Index");

            var urunler = await _context.Urunlers
                .Include(u => u.Restoran)
                .Where(u => u.RestoranId == id)
                .ToListAsync();

            ViewBag.RestoranAdi = _context.Restoranlars.Find(id)?.RestoranAdi;
            return View(urunler);
        }

        // 3. SEPETE EKLE: Arka planda çalışır, hafızaya atar
        public IActionResult SepeteEkle(int id)
        {
            var sepetJson = HttpContext.Session.GetString("Sepet");
            List<int> urunIdleri;

            if (sepetJson == null)
            {
                urunIdleri = new List<int>();
            }
            else
            {
                urunIdleri = JsonSerializer.Deserialize<List<int>>(sepetJson);
            }

            urunIdleri.Add(id);
            HttpContext.Session.SetString("Sepet", JsonSerializer.Serialize(urunIdleri));

            var urun = _context.Urunlers.Find(id);
            return RedirectToAction("Menu", new { id = urun.RestoranId });
        }

        // 4. SEPET EKRANI: Seçilenleri göster ve Onayla
        public async Task<IActionResult> Sepet()
        {
            var sepetJson = HttpContext.Session.GetString("Sepet");
            var sepetUrunleri = new List<Urunler>();

            if (sepetJson != null)
            {
                var urunIdleri = JsonSerializer.Deserialize<List<int>>(sepetJson);
                foreach (var id in urunIdleri)
                {
                    var urun = await _context.Urunlers.Include(u => u.Restoran).FirstOrDefaultAsync(u => u.UrunId == id);
                    if (urun != null) sepetUrunleri.Add(urun);
                }
            }

            
            // Giriş yapan müşteriyi hafızadan al
            var girisYapanMusteriId = HttpContext.Session.GetInt32("MusteriId");

            // Sayfaya bilgileri gönder
            ViewBag.MusteriId = girisYapanMusteriId;
            ViewBag.MusteriAd = HttpContext.Session.GetString("MusteriAd");

            // Eğer giriş yapılmadıysa manuel seçim için listeyi yine de doldur
            ViewBag.Kullanicilar = new SelectList(_context.Kullanicilars, "KullaniciId", "AdSoyad", girisYapanMusteriId);
            

            return View(sepetUrunleri);
        }

        // 5. SİPARİŞİ TAMAMLA 
        [HttpPost]
        public async Task<IActionResult> SiparisiTamamla(int? kullaniciId, decimal toplamTutar)
        {
            
            // Formdan ID gelmediyse Sessiondan al 
            if (kullaniciId == null)
            {
                kullaniciId = HttpContext.Session.GetInt32("MusteriId");
            }

            // Eğer hala ID yoksa işlemi iptal et ve başa dön
            if (kullaniciId == null) return RedirectToAction("Index");
            

            var sepetJson = HttpContext.Session.GetString("Sepet");
            if (sepetJson == null) return RedirectToAction("Index");

            var urunIdleri = JsonSerializer.Deserialize<List<int>>(sepetJson);

            // A) Sipariş Fişini Oluştur
            var ilkUrun = _context.Urunlers.Find(urunIdleri[0]);

            Siparisler yeniSiparis = new Siparisler
            {
                KullaniciId = (int)kullaniciId, 
                RestoranId = ilkUrun.RestoranId,
                Tarih = DateTime.Now,
                Durum = "Sipariş Alındı",
                ToplamTutar = toplamTutar
            };

            _context.Siparislers.Add(yeniSiparis);
            await _context.SaveChangesAsync();

            // B) Detayları Ekle
            var gruplanmisUrunler = urunIdleri.GroupBy(x => x);

            foreach (var grup in gruplanmisUrunler)
            {
                var urunId = grup.Key;
                var adet = grup.Count();
                var urunDetay = _context.Urunlers.Find(urunId);

                SiparisDetay detay = new SiparisDetay
                {
                    SiparisId = yeniSiparis.SiparisId,
                    UrunId = urunId,
                    Adet = adet,
                    BirimFiyat = urunDetay.Fiyat
                };
                _context.SiparisDetays.Add(detay);
            }

            await _context.SaveChangesAsync();

            // C) Sepeti Boşalt
            HttpContext.Session.Remove("Sepet");

            // Sipariş bitti, müşteriyi Vitrine (Restoranlara) geri gönder
            return RedirectToAction("Index");
        }
    }
}