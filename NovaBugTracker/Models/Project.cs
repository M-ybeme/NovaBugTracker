using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovaBugTracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "{0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Name { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "{0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        public int ProjectPriorityId { get; set; }


        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFormFile { get; set; }
        public string? ImageFileName { get; set; }
        public string? ImageFileType { get; set; }
        public bool Archived { get; set; }

        // nav properties
        public virtual Company? Company { get; set; }
        public virtual ProjectPriority? ProjectPriority { get; set; }
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();


    }
}
