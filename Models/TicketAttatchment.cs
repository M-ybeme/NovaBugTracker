using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using NovaBugTracker.Extensions;
using System.ComponentModel;

namespace NovaBugTracker.Models
{
    public class TicketAttatchment
    {
        public int Id { get; set; }

        [StringLength(200, ErrorMessage = "{0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }
        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }

        [NotMapped]
        [DisplayName("Select a file")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
        public IFormFile? ImageFormFile { get; set; }
        public string? ImageFileName { get; set; }
        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }


        // nav properties
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
