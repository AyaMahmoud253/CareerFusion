using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web_API.Models
{
    public class ApplicationUser : IdentityUser
    {
       
        public string FullName { get; set; }

        public string ?Title { get; set; }
        public string ?ProfilePicturePath { get; set; }
        public int ?FollowersCount { get; set; }
        public string? Description { get; set; } // Add Description property
        public string? Address { get; set; }    // Add Address property

        // Collections for skills, projects, and courses

        public virtual ICollection<Course>? Courses { get; set; }
        public virtual ICollection<Skill>? Skills { get; set; }
        public virtual ICollection<ProjectLink> ?ProjectLinks { get; set; }
        public virtual ICollection<SiteLink>? SiteLinks { get; set; } // Add SiteLink collection


    }
}
