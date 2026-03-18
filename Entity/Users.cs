using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;

namespace SupportDeskAPI.Entity
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public UserRole? UserRole { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Tickets>? Tickets { get; set; } = new List<Tickets>();
    }

    public class Tickets
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TicketId { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public TicketPriority? Priority { get; set; }
        public TicketStatus? Status { get; set; }
        public DateTime? Created { get; set; }

        public long UserId { get; set; }
        public Users User { get; set; }

        public long AssignedToUserId { get; set; }
        public Users AssignedToUser { get; set; }

        public ICollection<TicketComments> TicketComments { get; set; } = new List<TicketComments>();
        public ICollection<TicketsStatusHistory> TicketsStatusHistories { get; set; } = new List<TicketsStatusHistory>();
    }

    public class TicketsStatusHistory 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long TicketId { get; set; }
        public Tickets Ticket { get; set; }

        public TicketStatus? TicketStatus { get; set; }
        public DateTime? ChangeAt { get; set; }

        public long UserId { get; set; }
        public Users User { get; set; }
    }

    public class TicketComments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CommentsId { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedAt { get; set; }

        public long TicketId { get; set; }
        public Tickets Ticket { get; set; }

        public long UserId { get; set; }
        public Users User { get; set; }
    }

    public enum TicketPriority
    {
        Low, Medium, High
    }

    public enum TicketStatus
    {
        Open, InProgress, Closed
    }

    public enum UserRole
    {
        Admin, User
    }
}
