using VibeResume.Models;

public interface IPdfService
{
    byte[] GenerateResumePdf(ResumeViewModel model);
}