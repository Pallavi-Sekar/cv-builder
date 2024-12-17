using System.ComponentModel.DataAnnotations;

namespace CVBuilder.Models
{
    public class Education
    {
        public int Id { get; set; }

        public string? Degree { get; set; }
        public string? Institution { get; set; }

        public string? GraduationYear { get; set; }

        public int PersonalDetailsId { get; set; }  // Foreign Key

        public bool IsDraft { get; set; } // Indicates whether this is a draft
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}
