using SupportDeskAPI.Entity;
using System.ComponentModel.DataAnnotations;

namespace SupportDeskAPI.Dto
{
    public class TicketDto
    {
        [Required(ErrorMessage = "Subject is required")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Ticket priority is required")]
        [EnumDataType(typeof(TicketPriority), ErrorMessage = "Invalid ticket priority")]
        public TicketPriority? TicketPriority { get; set; }
    }

    public class TicketDetails
    {
        public long TicketNumber { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public TicketPriority? Priority { get; set; }

        public TicketStatus? Status { get; set; }

        public DateTime? CreatedDate { get; set; }

        public long AssignedAdmin { get; set; }
    }

    public class UpdateTicketDto
    {
        [Required(ErrorMessage = "Assigned admin is required")]
        public string? AssignedAdminEmail { get; set; }

        [Required(ErrorMessage = "Ticket status is required")]
        [EnumDataType(typeof(TicketStatus), ErrorMessage = "Invalid status value")]
        public TicketStatus? Status { get; set; }

        // [Required(ErrorMessage = "Tikect Comment is required")]
        public string? Comment { get; set; }
    }

    public class AssignTicketRequest 
    {
        public long UserId { get; set; }
        public long TicketId { get; set; }
        public TicketStatus TicketStatus { get; set; }
        public string? Comment { get; set; }
    }

    public class TicketStatusUpdateRequest
    {
        public long TicketId { get; set; }
        public TicketStatus TicketStatus { get; set; }
    }

    public class TicketCommentRequest
    {
        public long TicketId { get; set; }
        public string? Comment { get; set; }
    }
}
