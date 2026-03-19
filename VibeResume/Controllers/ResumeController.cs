using Microsoft.AspNetCore.Mvc;
using VibeResume.Models;
using VibeResume.Services;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using VibeResume.Models;
using VibeResume.Data;
using Microsoft.EntityFrameworkCore;

namespace VibeResume.Controllers

{
    public class ResumeController : Controller
    {
        private readonly IAIService _aiService;
        private readonly IPdfService _pdfService;
        private readonly AppDbContext _context;

        public ResumeController(IAIService aiService, IPdfService pdfService, AppDbContext context)
        {
            _aiService = aiService;
            _pdfService = pdfService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ResumeViewModel());
        }
        [HttpGet]
        public IActionResult History()
        {
            var histories = _context.ResumeHistories
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
            return View(histories);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ResumeViewModel model)
        {
            // YENİ: Mükerrer olmayan yetkinlik havuzu
            var skillPool = new HashSet<string>();
            string cvLanguage = model.CvLanguage ?? "Türkçe";

            // Özet (Hakkımda) kısmını işleyelim
            if (!string.IsNullOrEmpty(model.Summary))
            {
                var result = await _aiService.InterpretDescriptionAsync(model.Summary, model.JobTitle, model.TargetJobDescription, cvLanguage);
                model.Summary = result.RefinedText; // Paketten metni aldık
                if (result.ExtractedSkills != null) skillPool.UnionWith(result.ExtractedSkills);
            }

            // Deneyimleri işleyelim
            if (model.Experiences != null)
            {
                foreach (var exp in model.Experiences)
                {
                    // Tarih mantığı
                    string start = string.IsNullOrWhiteSpace(exp.StartDate) ? "?" : exp.StartDate;
                    string end = exp.IsCurrent ? "Günümüz" : (string.IsNullOrWhiteSpace(exp.EndDate) ? "?" : exp.EndDate);
                    exp.Duration = $"{start} - {end}";

                    // AI'dan paketi al
                    var result = await _aiService.InterpretDescriptionAsync(exp.Description, exp.JobTitle, model.TargetJobDescription, cvLanguage);

                    // BURASI ÖNEMLİ: Paketin içindeki RefinedText'i atıyoruz
                    exp.AIProfessionalSummary = result.RefinedText;

                    // Yetkinlikleri havuza at
                    if (result.ExtractedSkills != null)
                    {
                        skillPool.UnionWith(result.ExtractedSkills);
                    }
                }
            }

            // Projeleri işleyelim
            if (model.Projects != null)
            {
                foreach (var project in model.Projects)
                {
                    var result = await _aiService.InterpretDescriptionAsync(project.UserStory, project.TechStack, model.TargetJobDescription, cvLanguage);

                    project.AIProfessionalStory = result.RefinedText;

                    if (result.ExtractedSkills != null)
                    {
                        skillPool.UnionWith(result.ExtractedSkills);
                    }
                }
            }

            // YENİ: Havuzda toplanan tüm yetkinlikleri ana modele aktar
            model.TotalSoftSkills = skillPool.ToList();

            return View("Preview", model);
        }

        [HttpPost]
        public IActionResult DownloadPdf(ResumeViewModel model)
        {
            var pdfBytes = _pdfService.GenerateResumePdf(model);

            // wwwroot/resumes klasörüne kaydet
            var resumesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes");
            if (!Directory.Exists(resumesFolder))
                Directory.CreateDirectory(resumesFolder);

            var fileName = $"{model.FullName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(resumesFolder, fileName);
            System.IO.File.WriteAllBytes(filePath, pdfBytes);

            // SQL'e kaydet
            var history = new ResumeHistory
            {
                FullName = model.FullName,
                JobTitle = model.JobTitle,
                FilePath = $"/resumes/{fileName}",
                CreatedAt = DateTime.Now
            };
            _context.ResumeHistories.Add(history);
            _context.SaveChanges();

            return File(pdfBytes, "application/pdf", $"{model.FullName}_CV.pdf");
        }

        [HttpPost]
        public async Task<IActionResult> GenerateCoverLetter(ResumeViewModel model)
        {
            var coverLetter = await _aiService.GenerateCoverLetterAsync(model);
            ViewBag.CoverLetter = coverLetter;
            return View("CoverLetter", model);
        }
    }
}