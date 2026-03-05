using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YemekSepetiWeb.Models;

namespace YemekSepetiWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly YemekSepetiDbContext _context;

        public LoginController(YemekSepetiDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["Kullanicilar"] = new SelectList(_context.Kullanicilars, "KullaniciId", "AdSoyad");
            return View();
        }

        // 1. YÖNETİCİ GİRİŞİ
        [HttpPost]
        public IActionResult AdminLogin(string username, string password)
        {
            if (username == "admin" && password == "1234")
            {
                HttpContext.Session.SetString("UserRole", "Admin");
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Hata = "Hatalı Kullanıcı Adı veya Şifre!";
            ViewData["Kullanicilar"] = new SelectList(_context.Kullanicilars, "KullaniciId", "AdSoyad");
            return View("Index");
        }

        // 2. VAR OLAN MÜŞTERİ GİRİŞİ 
        [HttpPost]
        public IActionResult MusteriLogin(int kullaniciId, string sifre)
        {
            // Kullanıcıyı veritabanından bul
            var musteri = _context.Kullanicilars.Find(kullaniciId);

            // Kullanıcı bulunduysa VE şifresi doğruysa
            if (musteri != null && musteri.Sifre == sifre)
            {
                HttpContext.Session.SetString("UserRole", "Musteri");
                HttpContext.Session.SetInt32("MusteriId", musteri.KullaniciId);
                HttpContext.Session.SetString("MusteriAd", musteri.AdSoyad);

                return RedirectToAction("Index", "Musteri");
            }

            // Şifre yanlışsa veya kullanıcı yoksa
            ViewBag.MusteriHata = "Şifre Yanlış!";
            ViewData["Kullanicilar"] = new SelectList(_context.Kullanicilars, "KullaniciId", "AdSoyad");
            return View("Index");
        }

        // 3. HIZLI KAYIT VE GİRİŞ
        [HttpPost]
        public async Task<IActionResult> HizliKayit(string adSoyad, string telefon, string adres, string email, string sifre)
        {
            if (string.IsNullOrEmpty(adSoyad) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sifre))
            {
                ViewBag.Hata = "Lütfen İsim, E-posta ve Şifre alanlarını doldurunuz.";
                ViewData["Kullanicilar"] = new SelectList(_context.Kullanicilars, "KullaniciId", "AdSoyad");
                return View("Index");
            }

            // Yeni müşteriyi oluştur
            Kullanicilar yeniMusteri = new Kullanicilar
            {
                AdSoyad = adSoyad,
                Telefon = telefon ?? "555-0000",
                Adres = adres ?? "Hızlı Kayıt Adresi",
                Email = email,
                Sifre = sifre
            };

            // Veritabanına kaydet
            _context.Kullanicilars.Add(yeniMusteri);
            await _context.SaveChangesAsync();

            // Kayıt bitti şimdi bu yeni kişiyle giriş yapalım
            HttpContext.Session.SetString("UserRole", "Musteri");
            HttpContext.Session.SetInt32("MusteriId", yeniMusteri.KullaniciId);
            HttpContext.Session.SetString("MusteriAd", yeniMusteri.AdSoyad);

            return RedirectToAction("Index", "Musteri");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}