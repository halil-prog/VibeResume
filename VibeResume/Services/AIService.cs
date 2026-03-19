using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VibeResume.Models; // AIResult burada değilse yolunu düzelt

namespace VibeResume.Services
{
    public interface IAIService
    {
        // ARTIK Task<string> DEĞİL, Task<AIResult> DÖNÜYORUZ
        Task<AIResult> InterpretDescriptionAsync(string rawText, string techStack, string jobDescription, string cvLanguage = "Türkçe");
        Task<string> GenerateCoverLetterAsync(ResumeViewModel model);
    }

        public class AIService : IAIService
        {
            private readonly string _apiKey;
            private readonly HttpClient _httpClient;

            public AIService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
            {
                _apiKey = configuration["Gemini:ApiKey"];
                _httpClient = httpClientFactory.CreateClient();
            }

            public async Task<AIResult> InterpretDescriptionAsync(string rawText, string techStack, string jobDescription, string cvLanguage = "Türkçe")
            {
                if (string.IsNullOrWhiteSpace(rawText))
                    return new AIResult { RefinedText = rawText };

                string profession = string.IsNullOrWhiteSpace(techStack) ? "profesyonel iş dünyası" : techStack;

                // AI'ya hem JSON formatını hem de "Satır Arası Boşluk" (Escape Character) kuralını sertçe söylüyoruz
                string language = jobDescription?.Contains("İngilizce") == true ? "İngilizce" : "Türkçe";
                var prompt = $@"Sen uzman bir CV yazarısın. Cevabını SADECE ve SADECE geçerli JSON formatında ver. Açıklama ekleme, markdown kullanma.
CV DİLİ: {(cvLanguage == "İngilizce" ? "5. ExtractedSkills: Write soft skills in English (Problem Solving, Teamwork etc.)" : "5. ExtractedSkills: Metinden çıkan soft skill'leri Türkçe yaz (Problem Çözme, Takım Çalışması vb.)")}
Aşağıdaki ham metni profesyonel CV diline çevir.

MADDE SAYISI KURALI:
- Eğer metin basit, tek bir iş veya görev içeriyorsa: 2-3 madde yaz
- Kesinlikle MAKSIMUM 4 madde yaz. İstisna yok.
- 4 maddeden fazlası kesinlikle yasak.
- Her madde maksimum 12 kelime olsun. Kesinlikle.

YAZIM KURALLARI:
1. Her madde '• ' ile başlasın, maddeler \n ile ayrılsın
2. Her madde 1 cümle olsun, güçlü fiille bitsin (Geliştirdim, Kurdum, Tasarladım, Çözdüm...)
3. Teknik detayları koru ama hikaye anlatma, sonuç odaklı yaz
4. Metin içinde asla çift tırnak kullanma, tek tırnak kullan

5. ExtractedSkills: Metinden çıkan soft skill'leri Türkçe yaz (Problem Çözme, Takım Çalışması vb.)
{(!string.IsNullOrWhiteSpace(jobDescription)
        ? $"6. İş ilanı mevcut — anahtar kelimeleri ve beklentileri maddelere yansıt. İlan: {jobDescription}"
        : "6. İş ilanı yok — genel profesyonel CV dili kullan.")}

Alan/Pozisyon: {profession}
Ham Veri: {rawText}

Döndür (sadece bu JSON): {{""RefinedText"": ""• Madde 1\n• Madde 2"", ""ExtractedSkills"": [""Problem Çözme"", ""Takım Çalışması""]}}";

                var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };

                try
                {
                    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                    var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={_apiKey}";

                    var response = await _httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(responseString);
                        var aiRawText = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString().Trim();

                        // JSON'u temizleyip ayıklayan kısım
                        if (aiRawText.Contains("```json"))
                        {
                            aiRawText = aiRawText.Substring(aiRawText.IndexOf("```json") + 7);
                            aiRawText = aiRawText.Substring(0, aiRawText.LastIndexOf("```")).Trim();
                        }
                        else if (aiRawText.Contains("```"))
                        {
                            aiRawText = aiRawText.Substring(aiRawText.IndexOf("```") + 3);
                            aiRawText = aiRawText.Substring(0, aiRawText.LastIndexOf("```")).Trim();
                        }
                        aiRawText = aiRawText.Trim();

                        // JSON okurken oluşabilecek karakter hatalarını (tırnak vb.) ignore eden ayar
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, AllowTrailingCommas = true };
                        return JsonSerializer.Deserialize<AIResult>(aiRawText.Trim(), options) ?? new AIResult { RefinedText = rawText };
                    }

                    return new AIResult { RefinedText = "API Hatası: " + response.StatusCode };
                }
                catch (Exception ex)
                {
                    // BURASI ÇOK ÖNEMLİ: Eğer JSON yine de bozulursa, uygulamayı çökertme, 
                    // gelen çiğ metni RefinedText içine koy ve öyle dön.
                    return new AIResult { RefinedText = "Metin İşleme Hatası: " + ex.Message };
                }
            }
        public async Task<string> GenerateCoverLetterAsync(ResumeViewModel model)
        {
            var prompt = $@"Sen uzman bir kariyer danışmanısın. Aşağıdaki bilgileri kullanarak profesyonel bir ön yazı (cover letter) yaz.

KURALLAR:
1. 3-4 paragraf olsun
2. Samimi ama profesyonel bir dil kullan
3. Adayın güçlü yönlerini ve deneyimlerini öne çıkar
4. {(string.IsNullOrWhiteSpace(model.TargetJobDescription) ? "Genel bir ön yazı yaz." : "İş ilanına göre uyarla.")}
5. {(model.CvLanguage == "İngilizce" ? "İngilizce yaz." : "Türkçe yaz.")}
6. Sadece ön yazı metnini döndür, başka bir şey yazma.

Ad Soyad: {model.FullName}
Pozisyon: {model.JobTitle}
Özet: {model.Summary}
{(string.IsNullOrWhiteSpace(model.TargetJobDescription) ? "" : $"İş İlanı: {model.TargetJobDescription}")}";

            var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={_apiKey}";
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);
                    return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString().Trim();
                }

                return "API Hatası: " + response.StatusCode;
            }
            catch (Exception ex)
            {
                return "Hata: " + ex.Message;
            }
        }
    }
    }
