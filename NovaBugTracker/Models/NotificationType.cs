using System.ComponentModel.DataAnnotations;

namespace NovaBugTracker.Models
{
    public class NotificationType
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "{0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Name { get; set; }
    }
}
