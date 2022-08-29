using System.ComponentModel.DataAnnotations;

namespace NovaBugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string? PropertyName { get; set; }
        [StringLength(200, ErrorMessage = "{0} must be less than {1} characters long.")]
        public string? Description { get; set; }
        public DateTime Created { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        [Required]
        public string? UserId { get; set; }

        // nav properties
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
