using Microsoft.AspNetCore.Mvc.Rendering;

namespace NovaBugTracker.Models.ViewModels
{
    public class AsignDeveloperViewModel
    {
        public Ticket? Ticket { get; set; }

        public SelectList? DevList { get; set; }

        public string? DevID { get; set; }
    }
}
