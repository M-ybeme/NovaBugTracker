using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NovaBugTracker.Models
{
    public class BTUser : IdentityUser
    {

        [Required]
        [Display(Name = "FirstName")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and a max {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and a max {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }
        public string? FullName { get { return $"{FirstName} {LastName}"; } }
        public IFormFile? ImageFormFile { get; set; }
        public string? ImageFileName { get; set; }
        public string? ImageFileType { get; set; }
        public int CompanyId { get; set; }

        // nav properties
        public virtual Company? Company { get; set; }
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();



    }
}
