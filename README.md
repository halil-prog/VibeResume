# VibeResume 🚀

Gemini API ile ATS uyumlu CV üreten ASP.NET MVC uygulaması.

## Özellikler

- 📄 İş ilanına göre dinamik CV içeriği üretimi
- 🤖 Gemini AI ile deneyim ve proje açıklamalarını profesyonelleştirme
- 🎯 Otomatik soft skill extraction
- 🌍 Türkçe / İngilizce CV desteği
- ✉️ Ön yazı (cover letter) oluşturma
- 💾 Geçmiş CV'leri MSSQL'de saklama
- 📥 ATS uyumlu PDF indirme

## Teknolojiler

- ASP.NET MVC (.NET 9)
- Gemini API (gemini-2.5-flash-lite)
- QuestPDF
- Entity Framework Core
- MSSQL

## Kurulum

1. Repoyu klonla
2. `appsettings.json` dosyası oluştur:
```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=VibeResumeDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```
3. Migration çalıştır: `dotnet ef database update`
4. Uygulamayı başlat
