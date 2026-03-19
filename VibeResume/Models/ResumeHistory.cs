namespace VibeResume.Models
{
    public class ResumeHistory
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}