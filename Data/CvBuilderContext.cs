using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CVBuilder.Models
{
    public class CvBuilderContext : IdentityDbContext
{
    public CvBuilderContext(DbContextOptions<CvBuilderContext> options)
        : base(options)
    {
    }

        public DbSet<CVViewModel> CVViewModel { get; set; }

        public DbSet<PersonalDetails> PersonalDetails { get; set; }
        public DbSet<WorkExperience> WorkExperiences { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Language> Languages { get; set; }
    }
}
