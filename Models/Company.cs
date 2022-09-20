using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovaBugTracker.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and a max {1} characters long.", MinimumLength = 2)]
        [DisplayName("company Name")]
        public string? CompanyName { get; set; }
        public string? Description { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFile { get; set; }
        public string? ImageFileName { get; set; }
        public string? ImageFileType { get; set; }
        public byte[]? ImageFileData { get; set; }

        //nav properties

        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>(); 
    }
}
