using System.ComponentModel.DataAnnotations;

namespace NovaBugTracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ProjectPriorityId { get; set; }
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
