using System.ComponentModel.DataAnnotations;

namespace CVBuilder.Models
{
    public class WorkExperience
    {
        public int Id { get; set; }
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? Location { get; set; }
        public string? JobDescription { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; } // Can be null if currently employed

        public int PersonalDetailsId { get; set; }  // Foreign Key
        public bool IsDraft { get; set; } // Indicates whether this is a draft
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}
