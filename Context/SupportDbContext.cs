using Microsoft.EntityFrameworkCore;
using SupportDeskAPI.Entity;

namespace SupportDeskAPI.Context
{
    public class SupportDbContext : DbContext
    {

        public SupportDbContext(DbContextOptions<SupportDbContext> options) : base(options) 
        {

        }

        public DbSet<Users> users { get; set; }
        public DbSet<Tickets> tickets { get; set; }
        public DbSet<TicketsStatusHistory> ticketsStatusHistory { get; set; }
        public DbSet<TicketComments> ticketComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ticket creator relationship
            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.User)             // navigation property: ticket creator
                .WithMany(u => u.Tickets)       // a user can have many tickets
                .HasForeignKey(t => t.UserId)   // FK property
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket assigned-to relationship
            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.AssignedToUser)      // navigation property: assigned user
                .WithMany()                         // no navigation on Users side for assigned tickets
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
