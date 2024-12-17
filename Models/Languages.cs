using System.ComponentModel.DataAnnotations;

namespace CVBuilder.Models
{
    public class Language
    {
        public int Id { get; set; }
        public string? LanguageName { get; set; }
        public string? ProficiencyLevel { get; set; } 

        public int PersonalDetailsId { get; set; } 
        public bool IsDraft { get; set; } // Indicates whether this is a draft
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}
