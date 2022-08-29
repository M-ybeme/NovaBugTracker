using System.ComponentModel.DataAnnotations;

namespace NovaBugTracker.Models
{
    public class ProjectPriority
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
    }
}
