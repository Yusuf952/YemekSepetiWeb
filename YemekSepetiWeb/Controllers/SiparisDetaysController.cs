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
    // Siparişlerin içindeki kalemleri (Hangi üründen kaç adet, fiyatı ne kadar) yöneten kontrolcü.
    public class SiparisDetaysController : Controller
    {
        private readonly YemekSepetiDbContext _context;

        // Veritabanı bağlantısını (Dependency Injection ile) burada alıyoruz.
        public SiparisDetaysController(YemekSepetiDbContext context)
        {
            _context = context;
        }

        // Tüm sipariş detaylarının listelendiği ana ekran.
        // Burada ilişkili tablolardan (Siparis ve Urun) veri çekmek için Include kullandım.
        public async Task<IActionResult> Index()
        {
            var yemekSepetiDbContext = _context.SiparisDetays.Include(s => s.Siparis).Include(s => s.Urun);
            return View(await yemekSepetiDbContext.ToListAsync());
        }

        // Seçilen spesifik bir sipariş kaleminin detaylarını gösterir.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var siparisDetay = await _context.SiparisDetays
                .Include(s => s.Siparis)
                .Include(s => s.Urun)
                .FirstOrDefaultAsync(m => m.DetayId == id);
            if (siparisDetay == null)
            {
                return NotFound();
            }

            return View(siparisDetay);
        }

        // Yeni bir sipariş detayı ekleme sayfasını açar.
        // Dropdown (Açılır kutu) verilerini burada hazırlayıp View tarafına gönderiyorum.
        public IActionResult Create()
        {
            ViewData["SiparisId"] = new SelectList(_context.Siparislers, "SiparisId", "SiparisId");
            // DÜZELTME: Kullanıcı ürün seçerken ID değil, ürünün ismini görsün diye "UrunAdi" parametresini geçtim.
            ViewData["UrunId"] = new SelectList(_context.Urunlers, "UrunId", "UrunAdi");
            return View();
        }

        // Formdan gelen sipariş detay verisini veritabanına kaydeder (POST işlemi).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DetayId,SiparisId,UrunId,Adet,BirimFiyat")] SiparisDetay siparisDetay)
        {
            if (ModelState.IsValid)
            {
                _context.Add(siparisDetay);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Hata olursa formu tekrar doldururken dropdown'ları yeniden yüklüyorum.
            ViewData["SiparisId"] = new SelectList(_context.Siparislers, "SiparisId", "SiparisId", siparisDetay.SiparisId);
            ViewData["UrunId"] = new SelectList(_context.Urunlers, "UrunId", "UrunAdi", siparisDetay.UrunId);
            return View(siparisDetay);
        }

        // Mevcut bir sipariş kalemini düzenleme sayfasını getirir.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var siparisDetay = await _context.SiparisDetays.FindAsync(id);
            if (siparisDetay == null)
            {
                return NotFound();
            }
            ViewData["SiparisId"] = new SelectList(_context.Siparislers, "SiparisId", "SiparisId", siparisDetay.SiparisId);
            // Düzenleme sırasında da ürün isimlerinin doğru görünmesi için ayar.
            ViewData["UrunId"] = new SelectList(_context.Urunlers, "UrunId", "UrunAdi", siparisDetay.UrunId);
            return View(siparisDetay);
        }

        // Düzenlenen veriyi veritabanında günceller.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DetayId,SiparisId,UrunId,Adet,BirimFiyat")] SiparisDetay siparisDetay)
        {
            if (id != siparisDetay.DetayId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(siparisDetay);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Eşzamanlı işlem hatası kontrolü (Concurrency Check)
                    if (!SiparisDetayExists(siparisDetay.DetayId))
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
            ViewData["SiparisId"] = new SelectList(_context.Siparislers, "SiparisId", "SiparisId", siparisDetay.SiparisId);
            ViewData["UrunId"] = new SelectList(_context.Urunlers, "UrunId", "UrunAdi", siparisDetay.UrunId);
            return View(siparisDetay);
        }

        // Silme işlemi öncesi kullanıcıya onay ekranını gösterir.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var siparisDetay = await _context.SiparisDetays
                .Include(s => s.Siparis)
                .Include(s => s.Urun)
                .FirstOrDefaultAsync(m => m.DetayId == id);
            if (siparisDetay == null)
            {
                return NotFound();
            }

            return View(siparisDetay);
        }

        // Kullanıcı onayladıktan sonra kaydı veritabanından kalıcı olarak siler.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var siparisDetay = await _context.SiparisDetays.FindAsync(id);
            if (siparisDetay != null)
            {
                _context.SiparisDetays.Remove(siparisDetay);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Kaydın varlığını kontrol eden yardımcı metot.
        private bool SiparisDetayExists(int id)
        {
            return _context.SiparisDetays.Any(e => e.DetayId == id);
        }
    }
}