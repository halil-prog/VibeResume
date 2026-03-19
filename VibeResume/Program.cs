using QuestPDF.Infrastructure;
using VibeResume.Data;
using VibeResume.Services;
using VibeResume.Data;
using Microsoft.EntityFrameworkCore;
using VibeResume.Services;

// QuestPDF lisans ayarý (Topluluk sürümü)
QuestPDF.Settings.License = LicenseType.Community;



var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();
// 1. Sisteme genel HTTP fabrikasýný ekler
builder.Services.AddHttpClient();

// 2. Servislerini klasik yöntemle baðlar
builder.Services.AddScoped<IAIService, AIService>();


builder.Services.AddScoped<IPdfService, PdfService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
