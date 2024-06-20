using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Reflection.Emit;

namespace Web_API.Models
{
    public class ApplicationDBContext:IdentityDbContext<ApplicationUser>
    {
       
        public DbSet<ProjectLink> ProjectLinks { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<SiteLink> SiteLinks { get; set; } // Add DbSet for SiteLink
        public DbSet<TimelineStageEntity> TimelineStages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<JobFormEntity> JobForms { get; set; } // Add DbSet for JobFormEntity
        public DbSet<JobSkillEntity> JobSkills { get; set; } // Add DbSet for JobSkillEntity
        public DbSet<JobDescriptionEntity> JobDescriptions { get; set; } // Add DbSet for JobDescriptionEntity
        public DbSet<JobResponsibilityEntity> JobResponsibilities { get; set; } // Add DbSet for JobResponsibilityEntity
        public DbSet<JobFormCV> JobFormCVs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostPicture> PicturePosts { get; set; }
        public DbSet<PostCV> PostCVs { get; set; }
        public DbSet<TelephoneInterviewQuestionEntity> TelephoneInterviewQuestions { get; set; } // Add this DbSet
        public DbSet<PostFile> PostFiles { get; set; } // Add this DbSet for the new model

    


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the relationship between ApplicationUser and Course
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Courses)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId);
           
        }
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
           
        }
    }
}
