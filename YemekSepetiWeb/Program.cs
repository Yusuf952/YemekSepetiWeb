using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Veritabaný baðlantýsýný sisteme tanýtýyoruz:
builder.Services.AddDbContext<YemekSepetiWeb.Models.YemekSepetiDbContext>(options =>
    options.UseSqlServer("Server=localhost;Database=YemekSepetiDb;Trusted_Connection=True;TrustServerCertificate=True;"));

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- EKLEME 1 BAÞLANGIÇ: Session (Sepet) Servisi ---
// Sepetin çalýþmasý için hafýza servisini ekliyoruz.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Sepet 30 dakika hareketsiz kalýrsa silinsin
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// --- EKLEME 1 BÝTÝÞ ---

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- EKLEME 2 BAÞLANGIÇ: Session Middleware ---
// Uygulamanýn sepet özelliðini kullanmasýný aktif ediyoruz.
// YERÝ ÖNEMLÝ: UseRouting ile UseAuthorization arasýnda olmalý.
app.UseSession();
// --- EKLEME 2 BÝTÝÞ ---

app.UseAuthorization();

// --- DEÐÝÞÝKLÝK BURADA: Açýlýþ Sayfasý Ayarý ---
// controller=Home yerine controller=Login yaptýk.
// Böylece site açýlýnca direkt Giriþ Ekraný gelecek.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
