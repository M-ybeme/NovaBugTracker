using System.ComponentModel.DataAnnotations;

namespace NovaBugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "{0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Comment { get; set; }
        public DateTime Created { get; set; }
        public int TicketId { get; set; }
        [Required]
        public string? UserId { get; set; }

        // nav properties
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
