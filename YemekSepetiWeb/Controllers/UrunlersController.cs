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
    // Restoranlara ait menülerin ve ürünlerin (Fiyat, Açıklama vb.) yönetildiği kontrolcü sınıfı.
    public class UrunlersController : Controller
    {
        private readonly YemekSepetiDbContext _context;

        // Veritabanı bağlantısı yapıcı metot (Constructor) içerisinde alınıyor.
        public UrunlersController(YemekSepetiDbContext context)
        {
            _context = context;
        }

        // Tüm ürünlerin listelendiği ana sayfa.
        // Hangi ürünün hangi restorana ait olduğunu görebilmek için 'Include' metodu ile restoran verisini de çekiyorum.
        public async Task<IActionResult> Index()
        {
            var yemekSepetiDbContext = _context.Urunlers.Include(u => u.Restoran);
            return View(await yemekSepetiDbContext.ToListAsync());
        }

        // Seçilen spesifik bir ürünün detaylarını gösteren metot.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var urunler = await _context.Urunlers
                .Include(u => u.Restoran)
                .FirstOrDefaultAsync(m => m.UrunId == id);
            if (urunler == null) return NotFound();

            return View(urunler);
        }

        // Yeni ürün ekleme formunu açar.
        // Kullanıcı restoran seçebilsin diye restoran listesini hazırlayıp View tarafına gönderiyorum.
        public IActionResult Create()
        {
            ViewData["RestoranId"] = new SelectList(_context.Restoranlars, "RestoranId", "RestoranAdi");
            return View();
        }

        // Formdan gelen yeni ürün bilgisini veritabanına kaydeder (POST).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UrunId,RestoranId,UrunAdi,Aciklama,Fiyat")] Urunler urunler)
        {
            if (ModelState.IsValid)
            {
                _context.Add(urunler);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Hata durumunda (örneğin boş alan varsa) formu tekrar doldururken restoran listesini yeniden yüklüyorum.
            ViewData["RestoranId"] = new SelectList(_context.Restoranlars, "RestoranId", "RestoranAdi", urunler.RestoranId);
            return View(urunler);
        }

        // Mevcut bir ürünü düzenleme sayfasını getirir.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var urunler = await _context.Urunlers.FindAsync(id);
            if (urunler == null) return NotFound();

            ViewData["RestoranId"] = new SelectList(_context.Restoranlars, "RestoranId", "RestoranAdi", urunler.RestoranId);
            return View(urunler);
        }

        // Düzenlenen ürün bilgilerini günceller.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UrunId,RestoranId,UrunAdi,Aciklama,Fiyat")] Urunler urunler)
        {
            if (id != urunler.UrunId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(urunler);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Aynı anda birden fazla kişi düzenleme yaparsa çakışmayı kontrol ediyorum.
                    if (!UrunlerExists(urunler.UrunId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RestoranId"] = new SelectList(_context.Restoranlars, "RestoranId", "RestoranAdi", urunler.RestoranId);
            return View(urunler);
        }

        // Silme işlemi öncesi onay sayfasını gösterir.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var urunler = await _context.Urunlers
                .Include(u => u.Restoran)
                .FirstOrDefaultAsync(m => m.UrunId == id);
            if (urunler == null) return NotFound();

            return View(urunler);
        }

        // Gerçek silme işleminin yapıldığı yer.
        // KRİTİK NOKTA: İlişkisel veritabanı bütünlüğünü korumak için burada Try-Catch yapısı kullandım.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var urunler = await _context.Urunlers.FindAsync(id);
            if (urunler == null) return RedirectToAction(nameof(Index));

            try
            {
                _context.Urunlers.Remove(urunler);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                // Eğer bu ürün geçmiş siparişlerde yer alıyorsa, SQL Server silmeye izin vermeyecektir (Foreign Key hatası).
                // Programın çökmemesi için hatayı yakalayıp kullanıcıya anlaşılır bir uyarı veriyorum.
                ViewBag.Hata = "Bu ürün geçmiş siparişlerde yer aldığı için silinemiyor. (Veri Bütünlüğü Koruması)";
                return View("Delete", urunler);
            }
        }

        // Ürünün veritabanında var olup olmadığını kontrol eden yardımcı metot.
        private bool UrunlerExists(int id)
        {
            return _context.Urunlers.Any(e => e.UrunId == id);
        }
    }
}