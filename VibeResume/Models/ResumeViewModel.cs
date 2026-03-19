namespace VibeResume.Models
{
    public class ResumeViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }    // Yeni
        public string Phone { get; set; }    // Yeni
        public string Address { get; set; }  // Yeni
        public string LinkedInUrl { get; set; }
        public string GitHubUrl { get; set; }
        public string TargetJobDescription { get; set; }
        public string Degree { get; set; }
        public string JobTitle { get; set; } // Kullanıcı ne yazarsa o (Junior Dev, Biyomedikal Müh. vb.)
        public string Summary { get; set; }
        public string TechnicalSkills { get; set; }
        public string CvLanguage { get; set; } = "Türkçe";
        public string Skills { get; set; }
        public List<EducationInfo> Educations { get; set; } = new List<EducationInfo>();
        public List<string> TotalSoftSkills { get; set; } = new List<string>();

        public List<string> Languages { get; set; } = new List<string>();
        public List<CertificateItem> Certificates { get; set; } = new List<CertificateItem>();

        public List<WorkExperience> Experiences { get; set; } = new List<WorkExperience>();
        public List<UserProject> Projects { get; set; } = new List<UserProject>();

    }
    public class EducationInfo
    {
        public string SchoolName { get; set; }
        public string Department { get; set; }
        public string GraduationYear { get; set; }
        public string Degree { get; set; }
    }

    public class WorkExperience
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string JobTitle { get; set; }

        // --- YENİ EKLENEN TARİH ALANLARI ---
        public string StartDate { get; set; } // Formdan gelecek başlangıç tarihi
        public string EndDate { get; set; }   // Formdan gelecek bitiş tarihi
        public bool IsCurrent { get; set; }   // "Devam Ediyor" onay kutusu için

        public string Duration { get; set; }
        public string Description { get; set; }
        public string AIProfessionalSummary { get; set; }
    }

    public class UserProject
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TechStack { get; set; }
        public string ProjectLink { get; set; }
        public string UserStory { get; set; }
        public string AIProfessionalStory { get; set; }
    }
    public class CertificateItem
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
  
   
}