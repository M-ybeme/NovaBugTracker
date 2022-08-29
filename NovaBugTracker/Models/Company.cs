using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovaBugTracker.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        public string? CompanyName { get; set; }
        public string? Description { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFile { get; set; }
        public string? ImageFileName { get; set; }
        public string? ImageFileType { get; set; }

        //nav properties

        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>(); 
    }
}
