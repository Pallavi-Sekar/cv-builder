using System.ComponentModel.DataAnnotations;

namespace CVBuilder.Models
{
    public class PersonalDetails
    {
        public int Id { get; set; }

        public string? FullName { get; set; }

        public string? ContactNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public bool IsDraft { get; set; } // Indicates whether this is a draft
        public DateTime LastModified { get; set; } = DateTime.Now; 
    }
}
