using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SupportDeskAPI.Context;
using SupportDeskAPI.Dto;
using SupportDeskAPI.Entity;

namespace SupportDeskAPI.Services
{
    public class SupportDeskService
    {

        private readonly SupportDbContext _supportDbContext;

        public SupportDeskService(SupportDbContext supportDbContext)
        {
            _supportDbContext = supportDbContext;
        }

        public async Task<SupportDeskResponse<bool>> CretaeUserAsync(RegisterUser registerUser)
        {
            try
            {
                if (string.IsNullOrEmpty(registerUser.Email))
                    return SupportDeskResponse<bool>.Fail("ValidationError", "Email is required");

                if (string.IsNullOrEmpty(registerUser.Password))
                    return SupportDeskResponse<bool>.Fail("ValidationError", "Password is required");

                if (await _supportDbContext.users.AnyAsync(u => u.Email == registerUser.Email))
                    return SupportDeskResponse<bool>.Fail("AlreadyUserExist", "Already User Exist");

                var passwordHasher = new PasswordHasher<Users>();

                var newUser = new Users
                {
                    UserName = registerUser.UserName,
                    Email = registerUser.Email,
                    CreatedAt = DateTime.Now,
                    UserRole = registerUser.UserRole
                };

                newUser.PasswordHash = passwordHasher.HashPassword(newUser, registerUser.Password);

                await _supportDbContext.AddAsync(newUser);
                var isCreated = await _supportDbContext.SaveChangesAsync();

                return isCreated > 0 ? SupportDeskResponse<bool>.Ok(true) : SupportDeskResponse<bool>.Fail("ServerError", "User could not be created");
            }
            catch (ArgumentNullException argEx)
            {
                return SupportDeskResponse<bool>.Fail("ValidationError", argEx.Message);
            }
            catch (Exception ex)
            {
                return SupportDeskResponse<bool>.Fail("ServerError", "Internal Server Error");
            }
        }

        public async Task<SupportDeskResponse<bool>> CreateTicketAsync(TicketDto ticketDto, string email)
        {
            try
            {
                var user = await _supportDbContext.users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return SupportDeskResponse<bool>.Fail("InvalidUser", "User does not exist");

                if (string.IsNullOrEmpty(ticketDto.Subject))
                    return SupportDeskResponse<bool>.Fail("InvalidCredential", "Subject is required.");

                if (string.IsNullOrEmpty(ticketDto.Description))
                    return SupportDeskResponse<bool>.Fail("InvalidCredential", "Description is required.");

                var newTicket = new Tickets
                {
                    Subject = ticketDto.Subject,
                    Description = ticketDto.Description,
                    Priority = ticketDto.TicketPriority,
                    Status = TicketStatus.Open,
                    Created = DateTime.Now,
                    UserId = user.UserId
                };

                await _supportDbContext.tickets.AddAsync(newTicket);
                var ticketCreated = await _supportDbContext.SaveChangesAsync();

                return ticketCreated > 0 ? SupportDeskResponse<bool>.Ok(true) : SupportDeskResponse<bool>.Fail("ServerError", "Ticket could not be created");

            }
            catch (ArgumentNullException argEx)
            {
                return SupportDeskResponse<bool>.Fail("ValidationError", argEx.Message);
            }
            catch (Exception ex)
            {
                return SupportDeskResponse<bool>.Fail("ServerError", "Internal Server Error");
            }
        }

        public async Task<SupportDeskResponse<List<TicketListDto>>> GetAllTicketDetailsAsync(string email)
        {
            try
            {
                var user = await _supportDbContext.users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return SupportDeskResponse<List<TicketListDto>>.Fail("InvalidUSer", "User does not exist");

                // var ticketDetailsList = new List<TicketListDto>();
                var ticketList = new List<Tickets>();

                var ticketQuery = user.UserRole == UserRole.Admin
                ? _supportDbContext.tickets.AsQueryable()
                : _supportDbContext.tickets.Where(t => t.UserId == user.UserId);

                var ticketDetailsList = await ticketQuery
                .Select(t => new TicketListDto
                {
                    TicketNumber = t.TicketId,
                    Subject = t.Subject ?? string.Empty,
                    Description = t.Description ?? string.Empty,
                    Status = t.Status ?? TicketStatus.Open,
                    Priority = t.Priority ?? TicketPriority.Medium,
                    CreatedDate = t.Created,
                    AssignedAdmin = t.AssignedToUserId
                })
                .ToListAsync();

                if (ticketDetailsList.Count == 0)
                    return SupportDeskResponse<List<TicketListDto>>.Fail("NotFound", "No tickets found for current user");

                return SupportDeskResponse<List<TicketListDto>>.Ok(ticketDetailsList);

            }
            catch (Exception ex)
            {
                return SupportDeskResponse<List<TicketListDto>>.Fail("ServerError", "Internal Server Error");
            }
        }

        /*public async Task<SupportDeskResponse<bool>> AssignedTicketAsync(string email)
        {
            try
            {
                var userInfo = await _supportDbContext.users.FirstOrDefaultAsync(u => u.Email == email);
                if (userInfo.UserRole != UserRole.Admin)
                    return SupportDeskResponse<bool>.Fail("UnauthorizeAccess", "User haven't permission to perform acion");

                var updatedTicket = new Tickets
                {

                }
            }
        }*/
    }
}
