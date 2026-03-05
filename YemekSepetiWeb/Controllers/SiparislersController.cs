using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YemekSepetiWeb.Models;

namespace YemekSepetiWeb.Controllers
{
    // Müşterilerin verdiği siparişlerin (Tarih, Tutar, Durum vb.) ana yönetimini sağlayan kontrolcü.
    public class SiparislersController : Controller
    {
        private readonly YemekSepetiDbContext _context;

        // Dependency Injection ile veritabanı bağlamını (Context) sınıfa dahil ediyoruz.
        public SiparislersController(YemekSepetiDbContext context)
        {
            _context = context;
        }

        // Tüm siparişlerin listelendiği ana ekran.
        // Siparişin hangi kullanıcıya ve hangi restorana ait olduğunu görmek için ilişkili tabloları (Include) dahil ettim.
        public async Task<IActionResult> Index()
        {
            var yemekSepetiDbContext = _context.Siparislers.Include(s => s.Kullanici).Include(s => s.Restoran);
            return View(await yemekSepetiDbContext.ToListAsync());
        }

        // Seçilen özel bir siparişin detaylarını görüntüler.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var siparisler = await _context.Siparislers
                .Include(s => s.Kullanici)
                .Include(s => s.Restoran)
                .FirstOrDefaultAsync(m => m.SiparisId == id);
            if (siparisler == null) return NotFound();

            return View(siparisler);
        }

        // Yeni sipariş oluşturma sayfasını açar.
        // Kullanıcı ve Restoran seçimi için gerekli listeleri View tarafına taşıyorum.
        public IActionResult Create()
        {
            ViewData["KullaniciId"] = new SelectList(_context.Kullanicilars, "KullaniciId", "AdSoyad");
            ViewData["RestoranId"] = new SelectList(_context.Restoranlars, "RestoranId", "RestoranAdi");
            return View();
        }

        // Formdan gelen sipariş verisini veritabanına kaydeder (POST).
        // Eğer tarih veya durum bilgisi girilmediyse, kod tarafında varsayılan değerleri atıyorum.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SiparisId,KullaniciId,RestoranId,Tarih,ToplamTutar,Durum")] Siparisler siparisler)
        {
            if (ModelState.IsValid)
            {
                if (siparisler.Tarih == default) siparisler.Tarih = DateTime.Now;
                if (string.IsNullOrEmpty(siparisler.Durum)) siparisler.Durum = "Hazırlanıyor";

                _context.Add(siparisler);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KullaniciId"] = new SelectList(_context.Kullanicilars, "KullaniciId", "AdSoyad", siparisler.KullaniciId);
            ViewData["RestoranId"] = new SelectList(_context.Restoranlars, "RestoranId", "RestoranAdi", siparisler.RestoranId);
            return View(siparisler);
        }

        // Mevcut bir siparişin durumunu veya bilgilerini güncellemek için düzenleme sayfasını getirir.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var siparisler = await _context.Siparislers.FindAsync(id);
            if (siparisler == null) return NotFound();

            ViewData["KullaniciId"] = new SelectList(_context.Kullanicilars, "KullaniciId", "AdSoyad", siparisler.KullaniciId);
            ViewData["RestoranId"] = new SelectList(_context.Restoranlars, "RestoranId", "RestoranAdi", siparisler.RestoranId);
            return View(siparisler);
        }

        // Düzenlenen sipariş verisini veritabanında günceller.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SiparisId,KullaniciId,RestoranId,Tarih,ToplamTutar,Durum")] Siparisler siparisler)
        {
            if (id != siparisler.SiparisId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(siparisler);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SiparislerExists(siparisler.SiparisId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["KullaniciId"] = new SelectList(_context.Kullanicilars, "KullaniciId", "AdSoyad", siparisler.KullaniciId);
            ViewData["RestoranId"] = new SelectList(_context.Restoranlars, "RestoranId", "RestoranAdi", siparisler.RestoranId);
            return View(siparisler);
        }

        // Silme işlemi öncesi, kullanıcıya son kez 'Emin misiniz?' diye sorduğumuz ekran.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var siparisler = await _context.Siparislers
                .Include(s => s.Kullanici)
                .Include(s => s.Restoran)
                .FirstOrDefaultAsync(m => m.SiparisId == id);
            if (siparisler == null) return NotFound();

            return View(siparisler);
        }

        // PROJENİN EN KRİTİK KISMI: Trigger Korumalı Silme İşlemi
        // Veritabanındaki 'TRG_SiparisSilinemez' trigger'ı devreye girdiğinde programın çökmemesi için
        // burada özel bir Try-Catch bloğu kurguladım.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var siparisler = await _context.Siparislers.FindAsync(id);
            if (siparisler == null) return RedirectToAction(nameof(Index));

            try
            {
                // Önce ilişkili detay kayıtlarını temizliyorum (Data Integrity).
                var detaylar = _context.SiparisDetays.Where(d => d.SiparisId == id).ToList();
                _context.SiparisDetays.RemoveRange(detaylar);

                // Asıl siparişi silmeye çalışıyorum.
                // Eğer SQL Trigger aktifse buradan hata fırlatacak.
                _context.Siparislers.Remove(siparisler);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Veritabanından gelen hatayı yakalıyorum.
                // Eğer hata bizim yazdığımız Trigger'dan kaynaklıysa kullanıcıya özel mesaj gösteriyorum.
                if (ex.InnerException != null && ex.InnerException.Message.Contains("TRG_SiparisSilinemez"))
                {
                    ViewBag.Hata = "Güvenlik Trigger'ı Devreye Girdi: Siparişler güvenlik gereği silinemez, sadece durumu 'İptal' edilebilir!";
                }
                else
                {
                    ViewBag.Hata = "Bu kayıt silinemiyor. (İlişkili veri hatası)";
                }
                // Kullanıcıyı tekrar aynı sayfaya yönlendirip hatayı gösteriyorum.
                return View("Delete", siparisler);
            }
        }

        // Siparişin varlığını kontrol eden yardımcı metot.
        private bool SiparislerExists(int id)
        {
            return _context.Siparislers.Any(e => e.SiparisId == id);
        }
    }
}