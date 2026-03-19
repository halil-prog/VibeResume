namespace VibeResume.Models
{
    public class AIResult
    {
        public string RefinedText { get; set; } = "";
        public List<string> ExtractedSkills { get; set; } = new List<string>();
    }
}