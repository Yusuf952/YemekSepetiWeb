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
    // Web sitesindeki kullanıcıların (Müşteri veya Admin) kayıt, düzenleme ve silme işlemlerini yöneten sınıf.
    public class KullanicilarsController : Controller
    {
        private readonly YemekSepetiDbContext _context;

        // Yapıcı metot (Constructor): Veritabanı bağlantısını dependency injection ile buradan alıyorum.
        public KullanicilarsController(YemekSepetiDbContext context)
        {
            _context = context;
        }

        // Kullanıcıların listelendiği ana ekranı getirir.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Kullanicilars.ToListAsync());
        }

        // Seçilen bir kullanıcının detaylı profil bilgilerini gösterir.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kullanicilar = await _context.Kullanicilars
                .FirstOrDefaultAsync(m => m.KullaniciId == id);
            if (kullanicilar == null)
            {
                return NotFound();
            }

            return View(kullanicilar);
        }

        // Yeni üye kaydı oluşturma sayfasını açar.
        public IActionResult Create()
        {
            return View();
        }

        // Formdan gelen yeni kullanıcı verilerini veritabanına kaydeder (POST).
        // Güvenlik için AntiForgeryToken kontrolü var.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("KullaniciId,AdSoyad,Email,Sifre,Adres,Telefon,Rol")] Kullanicilar kullanicilar)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kullanicilar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kullanicilar);
        }

        // Mevcut kullanıcının bilgilerini düzenlemek için formu getirir.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kullanicilar = await _context.Kullanicilars.FindAsync(id);
            if (kullanicilar == null)
            {
                return NotFound();
            }
            return View(kullanicilar);
        }

        // Düzenlenen kullanıcı bilgilerini günceller.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("KullaniciId,AdSoyad,Email,Sifre,Adres,Telefon,Rol")] Kullanicilar kullanicilar)
        {
            if (id != kullanicilar.KullaniciId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kullanicilar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Aynı anda birden fazla admin düzenleme yaparsa çakışmayı önlemek için kontrol.
                    if (!KullanicilarExists(kullanicilar.KullaniciId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(kullanicilar);
        }

        // Kullanıcıyı silmeden önce onay ekranını gösterir.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kullanicilar = await _context.Kullanicilars
                .FirstOrDefaultAsync(m => m.KullaniciId == id);
            if (kullanicilar == null)
            {
                return NotFound();
            }

            return View(kullanicilar);
        }

        // Güvenli Silme İşlemi
        // Normalde silme yaparken veritabanındaki Trigger engel olabilir.
        // Bu yüzden programın hata verip kapanmasını engellemek için Try-Catch bloğu yazdım.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kullanicilar = await _context.Kullanicilars.FindAsync(id);
            if (kullanicilar == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Silme işlemini başlatıyorum.
                if (kullanicilar != null)
                {
                    // İlişkili siparişleri bulup temizlemeye çalışıyorum.
                    // (Bu adımda SQL Trigger devreye girip işlemi durdurabilir, bu beklediğimiz bir durum)
                    var siparisler = _context.Siparislers.Where(s => s.KullaniciId == id).ToList();
                    foreach (var siparis in siparisler)
                    {
                        var detaylar = _context.SiparisDetays.Where(d => d.SiparisId == siparis.SiparisId).ToList();
                        _context.SiparisDetays.RemoveRange(detaylar);
                        _context.Siparislers.Remove(siparis);
                    }

                    _context.Kullanicilars.Remove(kullanicilar);
                }

                // Değişiklikleri kaydetmeye çalıştığım an Trigger devreye girecek.
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Veritabanından fırlatılan hatayı burada yakalıyorum.
                // Eğer hata bizim yazdığımız 'TRG_SiparisSilinemez' triggerından geliyorsa kullanıcıya özel mesaj gösteriyorum.
                if (ex.InnerException != null && ex.InnerException.Message.Contains("TRG_SiparisSilinemez"))
                {
                    ViewBag.Hata = "Güvenlik Uyarısı: Bu kullanıcının sipariş geçmişi olduğu için silinmesi veritabanı Trigger'ı tarafından engellendi!";
                }
                else
                {
                    // Başka bir veritabanı hatası varsa 
                    ViewBag.Hata = "Bu kayıt silinemiyor çünkü başka verilerle ilişkili.";
                }

                // Hata mesajını ekrana basıp kullanıcıyı tekrar silme sayfasına yönlendiriyorum (Crash yok).
                return View("Delete", kullanicilar);
            }
        }
        

        // Kullanıcının veritabanında var olup olmadığını kontrol eden yardımcı metot.
        private bool KullanicilarExists(int id)
        {
            return _context.Kullanicilars.Any(e => e.KullaniciId == id);
        }
    }
}