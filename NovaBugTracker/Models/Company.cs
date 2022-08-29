using System.ComponentModel.DataAnnotations;

namespace NovaBugTracker.Models
{
    public class Company
    {
        public string? Id { get; set; }

        [Required]
        public string? CompanyName { get; set; }
        public string? Description { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string? ImageFileName { get; set; }
        public string? ImageFileType { get; set; }

        //nav properties

        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>(); 
    }
}
