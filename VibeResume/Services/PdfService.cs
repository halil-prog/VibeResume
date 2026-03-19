using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VibeResume.Models;

namespace VibeResume.Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateResumePdf(ResumeViewModel model)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana).LineHeight(1.2f));

                    // --- HEADER (Dinamik ve Ortalı) ---
                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text(model.FullName).FontSize(22).SemiBold();
                        col.Item().AlignCenter().Text(model.JobTitle).FontSize(13).Italic();
                        col.Item().AlignCenter().Text($"{model.Email} | {model.Phone} | {model.Address}").FontSize(9);

                        col.Item().AlignCenter().Row(row =>
                        {
                            row.Spacing(8);
                            if (!string.IsNullOrEmpty(model.LinkedInUrl))
                                row.AutoItem().Hyperlink(model.LinkedInUrl).Text("LinkedIn").FontColor(Colors.Blue.Medium).Underline().FontSize(9);

                            row.AutoItem().Text("|").FontSize(9);

                            if (!string.IsNullOrEmpty(model.GitHubUrl))
                                row.AutoItem().Hyperlink(model.GitHubUrl).Text("GitHub").FontColor(Colors.Blue.Medium).Underline().FontSize(9);
                        });
                        col.Item().PaddingTop(10);
                    });
                    bool isEng = model.CvLanguage == "İngilizce";
                    page.Content().Column(col =>
                    {
                        // --- HAKKIMDA (ÖZET) KISMI ---
                        if (!string.IsNullOrEmpty(model.Summary))
                        {
                            col.Item().PaddingTop(15).PaddingBottom(10).Text(model.Summary).FontSize(10).Italic();
                        }
                        // --- DENEYİM ---
                        if (model.Experiences != null && model.Experiences.Any())
                        {
                            col.Item().PaddingTop(10).AlignCenter().Text(isEng ? "EXPERIENCE" : "DENEYİM").FontSize(11).SemiBold();
                            col.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Black);

                            foreach (var exp in model.Experiences)
                            {
                                col.Item().PaddingVertical(5).Column(inner => {
                                    inner.Item().Row(row => {
                                        row.RelativeItem().Text($"{exp.CompanyName}, {exp.JobTitle}").Bold();
                                        row.RelativeItem().AlignRight().Text(exp.Duration).FontSize(10); // Tarih formdan geliyor
                                    });
                                    // Gereksiz parantezler silindi, sadece AI'nın profesyonelleştirdiği özet kaldı
                                    var lines = exp.AIProfessionalSummary?.Split('\n', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                                    foreach (var line in lines)
                                    {
                                        var cleanLine = line.Trim().TrimStart('•').Trim();
                                        inner.Item().PaddingTop(2).Text($"• {cleanLine}").FontSize(10);
                                    }
                                });
                            }
                        }

                        // --- PROJE / ÇALIŞMALAR ---
                        if (model.Projects != null && model.Projects.Any())
                        {
                            col.Item().PaddingTop(15).AlignCenter().Text(isEng ? "PROJECTS" : "PROJE VE ÇALIŞMALAR").FontSize(11).SemiBold();
                            col.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Black);

                            foreach (var proj in model.Projects)
                            {
                                col.Item().PaddingVertical(5).Column(inner => {
                                    inner.Item().Row(row => {
                                        row.RelativeItem().Text(proj.Title).Bold();
                                        row.RelativeItem().AlignRight().Text(proj.TechStack).FontSize(9).Italic(); // Yazılımcı için Tech, Mühendis için Araçlar
                                    });
                                    var projLines = proj.AIProfessionalStory?.Split('\n', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                                    foreach (var projLine in projLines)
                                    {
                                        var cleanProjLine = projLine.Trim().TrimStart('•').Trim();
                                        inner.Item().PaddingTop(2).Text($"• {cleanProjLine}").FontSize(10);
                                    }
                                });
                            }
                        }

                        // --- EĞİTİM ---
                        if (model.Educations != null && model.Educations.Any())
                        {
                            col.Item().PaddingTop(15).AlignCenter().Text(isEng ? "EDUCATION" : "EĞİTİM").FontSize(11).SemiBold();
                            col.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Black);

                            foreach (var edu in model.Educations)
                            {
                                col.Item().PaddingVertical(5).Column(inner => {
                                    inner.Item().Row(row => {
                                        row.RelativeItem().Text($"{edu.SchoolName} | {edu.Department}").Bold();
                                        row.RelativeItem().AlignRight().Text(edu.GraduationYear).FontSize(10); // Tarih formdan geliyor
                                    });
                                    // Statik "Ön Lisans" yazısı kalktı, modelden gelen Degree basılıyor
                                    if (!string.IsNullOrEmpty(edu.Degree))
                                        inner.Item().Text(edu.Degree).FontSize(9).Italic();
                                });
                            }
                        }

                        if (!string.IsNullOrEmpty(model.TechnicalSkills))
                        {
                            col.Item().PaddingTop(15).AlignCenter().Text(isEng ? "TECHNICAL SKILLS" : "TEKNİK YETKİNLİKLER").FontSize(11).SemiBold();
                            col.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Black);
                            col.Item().PaddingTop(5).AlignCenter().Text(model.TechnicalSkills).FontSize(10);
                        }

                        if (model.TotalSoftSkills != null && model.TotalSoftSkills.Any())
                        {
                            col.Item().PaddingTop(15); // Biraz boşluk bırakalım

                            // BAŞLIK (Siyah, Ortalanmış, Büyük Harf)
                            col.Item().AlignCenter().Text(isEng ? "PERSONAL COMPETENCIES" : "KİŞİSEL YETKİNLİKLER").FontSize(12).Bold().FontColor(Colors.Black);
                            col.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Black);

                            // YETKİNLİK HAVUZU (Yan yana dizilen şık metin)
                            col.Item().PaddingTop(5).AlignCenter().Text(text =>
                            {
                                // Yetkinlikleri "İletişim • Problem Çözme • Takım Çalışması" şeklinde birleştiriyoruz
                                var skillsText = string.Join("  •  ", model.TotalSoftSkills);
                                text.Span(skillsText).FontSize(10).Italic().FontColor(Colors.Grey.Darken3);
                            });

                        // --- SERTİFİKALAR & REFERANSLAR ---
                        if (model.Certificates != null && model.Certificates.Any(c => !string.IsNullOrWhiteSpace(c.Name)))
                        {
                                col.Item().PaddingTop(15).AlignCenter().Text(isEng ? "CERTIFICATES & REFERENCES" : "SERTİFİKALAR VE REFERANSLAR").FontSize(11).SemiBold();
                                col.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Black);

                            foreach (var cert in model.Certificates.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
                            {
                                col.Item().PaddingBottom(2).AlignCenter().Text(text =>
                                {
                                    if (!string.IsNullOrWhiteSpace(cert.Url))
                                    {
                                        text.Hyperlink("• " + cert.Name, cert.Url)
                                            .FontSize(10)
                                            .FontColor(Colors.Blue.Darken2)
                                            .Underline();
                                    }
                                    else
                                    {
                                        text.Span("• " + cert.Name).FontSize(10);
                                    }
                                });
                            }
                            col.Item().PaddingBottom(10);
}
if (model.Languages != null && model.Languages.Any(l => !string.IsNullOrWhiteSpace(l)))
{
                                col.Item().PaddingTop(15).AlignCenter().Text(isEng ? "LANGUAGES" : "YABANCI DİL").FontSize(11).SemiBold();
                                col.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Black);

    var validLanguages = model.Languages.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
    col.Item().AlignCenter().Text(string.Join("  |  ", validLanguages)).FontSize(10);
}

                       
                        }
                    });
                });
            }).GeneratePdf();
        }
    }
}