using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace NovaBugTracker.Models
{
    public class Invite
    {
        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? JoinDate { get; set; }
        public Guid CompanyToken { get; set; }
        public int CompanyId { get; set; }
        public int ProjectId { get; set; }

        [Required]
        public string? InvitorId { get; set; }
        public string? InviteeId { get; set; }

        [Required]
        [Display(Name = "Recipient's Email")]
        [DataType(DataType.EmailAddress)]
        public string? InviteeEmail { get; set; }


        [Required]
        [StringLength(500, ErrorMessage = "{0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? InviteeFirstName { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "{0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? InviteeLastName { get; set; }

        [StringLength(500, ErrorMessage = "{0} must be at least {2} and at most {1} characters long.",MinimumLength = 2)]
        public string? Message { get; set; }
        public bool IsValid { get; set; }

        // nav properties
        public virtual Company? Company { get; set; }
        public virtual Project? Project { get; set; }
        public virtual BTUser? Invitor { get; set; }
        public virtual BTUser? Invitee { get; set; }

    }
}
