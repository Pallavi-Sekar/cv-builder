using System.ComponentModel.DataAnnotations;

namespace CVBuilder.Models
{
    public class Skill
    {
        public int Id { get; set; }

        public string? SkillName { get; set; }
        public string? ProficiencyLevel { get; set; } // Examples: Beginner, Intermediate, Advanced

        public int PersonalDetailsId { get; set; }  // Foreign Key

        public bool IsDraft { get; set; } // Indicates whether this is a draft
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}
