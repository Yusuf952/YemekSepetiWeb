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
    public class RestoranlarsController : Controller
    {
        private readonly YemekSepetiDbContext _context;

        // Veritabanı bağlantısını burada yapıcı metot ile alıyoruz.
        public RestoranlarsController(YemekSepetiDbContext context)
        {
            _context = context;
        }

        // Restoranların listelendiği ana sayfa (Index)
        // Veritabanındaki tüm restoranları asenkron olarak çekip listeye gönderiyorum.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Restoranlars.ToListAsync());
        }

        // Seçilen restoranın detay bilgilerini gösteren metot.
        // ID gelmezse veya restoran bulunamazsa hata sayfasına yönlendiriyorum.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var restoranlar = await _context.Restoranlars
                .FirstOrDefaultAsync(m => m.RestoranId == id);

            if (restoranlar == null) return NotFound();

            return View(restoranlar);
        }

        // Yeni restoran ekleme sayfasını açar
        public IActionResult Create()
        {
            return View();
        }

        // Formdan gelen verileri veritabanına kaydeder.
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RestoranId,RestoranAdi,Kategori,MinTutar,Puan,ResimYolu")] Restoranlar restoranlar)
        {
            if (ModelState.IsValid)
            {
                _context.Add(restoranlar);
                await _context.SaveChangesAsync();
                // Kayıt başarılıysa listeye geri dön.
                return RedirectToAction(nameof(Index));
            }
            return View(restoranlar);
        }

        // Mevcut bir restoranı düzenlemek için güncelleme sayfasını getirir.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var restoranlar = await _context.Restoranlars.FindAsync(id);
            if (restoranlar == null) return NotFound();

            return View(restoranlar);
        }

        // Düzenlenen bilgileri veritabanına işleyen metot.
        // Veri tutarlılığı için ID kontrolü yapılıyor.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RestoranId,RestoranAdi,Kategori,MinTutar,Puan,ResimYolu")] Restoranlar restoranlar)
        {
            if (id != restoranlar.RestoranId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(restoranlar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Aynı anda başkası düzenlemiş mi kontrolü
                    if (!RestoranlarExists(restoranlar.RestoranId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(restoranlar);
        }

        // Silme işlemi için kullanıcıya onay sayfasını gösterir.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var restoranlar = await _context.Restoranlars
                .FirstOrDefaultAsync(m => m.RestoranId == id);

            if (restoranlar == null) return NotFound();

            return View(restoranlar);
        }

        // Gerçek silme işleminin yapıldığı yer.
        // ÖNEMLİ: Burada ilişkili veri hatasını önlemek için Try-Catch bloğu kullandım.
        // Eğer restorana ait ürün veya sipariş varsa veritabanı hatasını yakalayıp kullanıcıya uyarı veriyorum.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var restoranlar = await _context.Restoranlars.FindAsync(id);
            if (restoranlar == null) return RedirectToAction(nameof(Index));

            try
            {
                _context.Restoranlars.Remove(restoranlar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // İlişkili veri hatası (Foreign Key Constraint) yakalandı.
                // Kullanıcıya anlayacağı dilden bir hata mesajı oluşturuyorum.
                ViewBag.Hata = "Bu restoran silinemiyor çünkü içinde kayıtlı ürünler veya geçmiş siparişler var. Önce onları temizlemelisiniz.";
                return View("Delete", restoranlar);
            }
        }

        // Restoranın veritabanında var olup olmadığını kontrol eden yardımcı metot.
        private bool RestoranlarExists(int id)
        {
            return _context.Restoranlars.Any(e => e.RestoranId == id);
        }
    }
}