
    namespace CVBuilder.Models
{
    public class CVViewModel
    {
        public int Id { get; set; }
        // Personal Details Section
        public  PersonalDetails PersonalDetails{ get; set; }

        // Work Experience Section
        public List<WorkExperience> WorkExperiences { get; set; }

        // Education Section
        public List<Education> Educations { get; set; }

        // Skills Section
        public List<Skill> Skills { get; set; }

        // Languages Section
        public List<Language> Languages { get; set; }

        public int SelectedTemplate { get; set; }

        // Save Draft Feature
        public int IsDraft { get; set; } // Indicates whether this is a draft
        public DateTime LastModified { get; set; } = DateTime.Now;

        // Constructor to initialize the lists
        public CVViewModel()
        {
            PersonalDetails = new PersonalDetails();
            WorkExperiences = new List<WorkExperience>();
            Educations = new List<Education>();
            Skills = new List<Skill>();
            Languages = new List<Language>();
        }
    }





   
}

